using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class MongoStorageProvider<TValue> : IStorageProvider<TValue>
{
    private readonly ILogger logger;
    private readonly MongoStoringOptions options;
    private readonly IMongoCollection<MongoRecord> collection;

    public MongoStorageProvider(
        ILogger<MongoStorageProvider<TValue>> logger,
        IOptions<MongoStoringOptions> options,
        IMongoDatabase database)
    {
        this.logger = logger;
        this.options = options.Value;
        this.collection = database.GetCollection<MongoRecord>(MongoNames.StorageCollectionName);
    }

    public async Task<ValueRecord> AddOrGet(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        CancellationToken token)
    {
        var strategy = options.InsertRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await FindOne(key, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} found.",
                    key.Id, found.Audit.Version, attempt);
                return found;
            }

            if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} added.",
                    key.Id, inserted.Audit.Version, attempt);
                return inserted;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({KeyId}): {Attempt} reached the limit.", key.Id, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrGet({KeyId}): {Attempt} failed.", key.Id, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<ValueRecord> AddOrUpdate(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token)
    {
        var strategy = options.UpsertRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrUpdate({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await FindOneAndReplace(key, updateFactory, token) is Some<ValueRecord>(var replaced))
            {
                logger.LogInformation("Storage.AddOrUpdate({KeyId}, {Version}): {Attempt} updated.",
                    key.Id, replaced.Audit.Version, attempt);
                return replaced;
            }

            if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
            {
                logger.LogInformation("Storage.AddOrUpdate({KeyId}, {Version}): {Attempt} added.",
                    key.Id, inserted.Audit.Version, attempt);
                return inserted;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({KeyId}): {Attempt} reached the limit.", key.Id, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrUpdate({KeyId}): {Attempt} failed.", key.Id, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) => FindOne(key, token);

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) => DeleteOne(key, token);

    public IQueryable<KeyRecord> GetKeys() =>
        collection.AsQueryable(new AggregateOptions()).Select(x => new KeyRecord(x.Key.Id, x.KeyType, x.KeyContent, x.Key.ValueType));

    public void Dispose() { /* The mongo client is DI managed. */ }

    private async Task<Option<ValueRecord>> FindOne(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("MongoDB({CollectionName}: {RecordId}) finding: begins.", collection.CollectionNamespace.FullName, key.Id);

        var found = await collection.Find(filter: x => x.Key == new Key(key.Id, key.ValueType), new FindOptions()).SingleOrDefaultAsync(token);

        if (found != null)
            logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] finding: succeeded.",
                collection.CollectionNamespace.FullName, key.Id, found.Version);
        else
            logger.LogDebug("MongoDB({CollectionName}: {RecordId}) finding: not found.",
                collection.CollectionNamespace.FullName, key.Id);

        return found.AsOption().MapOption(x => new ValueRecord(key.ValueType, x.ValueContent, new Audit(x.Details, x.Version)));
    }

    private async Task<Option<ValueRecord>> InsertOne(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token)
    {
        var added = await addFactory(key);
        var keyId = new Key(key.Id, key.ValueType);
        var addedRecord = new MongoRecord(
            keyId,
            key.Type,
            key.Content,
            added.Audit.Version,
            added.Content,
            added.Audit.Details);

        logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] inserting: begins.",
            collection.CollectionNamespace.FullName, key.Id, added.Audit.Version);

        try
        {
            await collection.InsertOneAsync(
                addedRecord,
                new InsertOneOptions(),
                token);

            logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] inserting : succeeded.",
                collection.CollectionNamespace.FullName, key.Id, added.Audit.Version);
            return Option.Some(added with {Audit = new Audit(added.Audit.Details, addedRecord.Version)});
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(ex, "MongoDB({CollectionName}: {RecordId})[{Version}] inserting: already present.",
                collection.CollectionNamespace.FullName, key.Id, added.Audit.Version);
            return Option.None;
        }
    }

    private async Task<Option<ValueRecord>> FindOneAndReplace(
        KeyRecord key,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token)
    {
        if (await FindOne(key, token) is not Some<ValueRecord>(var found))
            return Option.None;

        var updated = await updateFactory(key, found);
        var updatedRecord = new MongoRecord(
            new(key.Id, key.ValueType),
            key.Type,
            key.Content,
            updated.Audit.Version,
            updated.Content,
            updated.Audit.Details);

        logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] replacing: begins.",
            collection.CollectionNamespace.FullName, key.Id, updatedRecord.Version);

        var result = await collection.ReplaceOneAsync(
            filter: x => x.Key == new Key(key.Id,key.ValueType) && x.Version == found.Audit.Version,
            updatedRecord,
            new ReplaceOptions(),
            token);

        if (result.MatchedCount != 0)
        {
            logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] replacing: succeeded.",
                collection.CollectionNamespace.FullName, key.Id, updatedRecord.Version);
            return Option.Some(updated with {Audit = new Audit(updated.Audit.Details, updatedRecord.Version)});
        }

        logger.LogWarning("MongoDB({CollectionName}: {RecordId})[{Version}] replacing: outdated version.",
            collection.CollectionNamespace.FullName, key.Id, updatedRecord.Version);
        return Option.None;

    }

    private async Task<Option<ValueRecord>> DeleteOne(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("MongoDB({CollectionName}: {RecordId}) deleting: begins.", collection.CollectionNamespace.FullName, key.Id);

        var deleted = await collection.FindOneAndDeleteAsync<MongoRecord>(
            filter: x => x.Key == new Key(key.Id, key.ValueType),
            new FindOneAndDeleteOptions<MongoRecord>(),
            token);

        if (deleted != null)
            logger.LogDebug("MongoDB({CollectionName}: {RecordId})[{Version}] finding: succeeded.",
                collection.CollectionNamespace.FullName, key.Id, deleted.Version);
        else
            logger.LogDebug("MongoDB({CollectionName}: {RecordId}) finding: not found.",
                collection.CollectionNamespace.FullName, key.Id);

        return deleted.AsOption().MapOption(x => new ValueRecord(key.ValueType, x.ValueContent, new Audit(x.Details, x.Version)));
    }
}
