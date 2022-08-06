using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
        var keyId = new Key(key.Id, key.ValueType);

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({@Key}): {Attempt} begins.", keyId, attempt);

            if (await FindOne(keyId, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} found.",
                    keyId, found.Audit.Version, attempt);
                return found;
            }

            var added = await addFactory(key);
            var record = new MongoRecord(keyId, key.Type, key.Content, added.Content, added.Audit.Version, added.Audit.Details);
            if (await InsertOne(record, token))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} added.",
                    keyId, added.Audit.Version, attempt);
                return added;
            }

            logger.LogWarning("Storage.AddOrGet({@Key}): {Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

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
        var keyId = new Key(key.Id, key.ValueType);

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrUpdate({@Key}): {Attempt} begins.", keyId, attempt);
            if (await FindOne(keyId, token) is Some<ValueRecord>(var found))
            {
                var updated = await updateFactory(key, found);
                var replaceRecord = new MongoRecord(keyId, key.Type, key.Content, updated.Content, updated.Audit.Version, updated.Audit.Details);
                if (await ReplaceOne(replaceRecord, found.Audit.Version, token))
                {
                    logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): {Attempt} updated.",
                        keyId, updated.Audit.Version, attempt);
                    return updated;
                }
            }

            var added = await addFactory(key);
            var insertRecord = new MongoRecord(keyId, key.Type, key.Content, added.Content, added.Audit.Version, added.Audit.Details);
            if (await InsertOne(insertRecord, token))
            {
                logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): {Attempt} added.",
                    keyId, added.Audit.Version, attempt);
                return added;
            }

            logger.LogWarning("Storage.AddOrUpdate({@Key}): {Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryGet({@Key}): begins.", keyId);

        if (await FindOne(keyId, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogInformation("Storage.TryGet({@Key}, {Version}): found.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogInformation("Storage.TryGet({@Key}): not found.", keyId);
        return Option.None;
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryRemove({@Key}): begins.", keyId);

        if (await FindOneAndDelete(keyId, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogInformation("Storage.TryRemove({@Key}, {Version}): succeeded.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogInformation("Storage.TryRemove({@Key}): not found.", keyId);
        return Option.None;
    }

    public async IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, [EnumeratorCancellation] CancellationToken token)
    {
        logger.LogInformation("Storage.GetKeys(): begins.");

        using var cursor = await collection.Aggregate(new AggregateOptions())
            .Project(x => new KeyRecord(x.Key.Id, x.KeyType, x.Key.ValueType, x.KeyContent))
            .Match(predicate)
            .ToCursorAsync(token);
        while (await cursor.MoveNextAsync(token))
            foreach (var key in cursor.Current)
                yield return key;

        logger.LogInformation("Storage.GetKeys(): succeeded.");
    }

    private async Task<Option<ValueRecord>> FindOne(Key key, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}) finding: begins.", key);

        var found = await collection.Find(filter: x => x.Key == key, new FindOptions()).SingleOrDefaultAsync(token);

        if (found != null)
            logger.LogDebug("MongoDB({@Key}, {Version}) finding: succeeded.", key, found.Version);
        else
            logger.LogDebug("MongoDB({@Key}) finding: not found.", key);

        return found.AsOption().MapOption(x => new ValueRecord(x.ValueContent, new Audit(x.Details, x.Version)));
    }

    private async Task<bool> InsertOne(MongoRecord record, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, {Version}) inserting: begins.", record.Key, record.Version);

        try
        {
            await collection.InsertOneAsync(record, new InsertOneOptions(), token);

            logger.LogDebug("MongoDB({@Key}, {Version}) inserting : succeeded.", record.Key, record.Version);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(ex, "MongoDB({@Key}, {Version}) inserting: already present.", record.Key, record.Version);
            return false;
        }
    }

    private async Task<bool> ReplaceOne(MongoRecord record, long version, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, {Version}) replacing: begins.", record.Key, record.Version);

        var result = await collection.ReplaceOneAsync(x => x.Key == record.Key && x.Version == version, record, new ReplaceOptions(), token);
        if (result.MatchedCount != 0)
        {
            logger.LogDebug("MongoDB({@Key}, {Version}) replacing: succeeded.", record.Key, record.Version);
            return true;
        }

        logger.LogWarning("MongoDB({@Key}, {Version}) replacing: outdated version.", record.Key, record.Version);
        return false;
    }

    private async Task<Option<ValueRecord>> FindOneAndDelete(Key key, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}) deleting: begins.", key);

        var deleted = await collection.FindOneAndDeleteAsync(
            filter: x => x.Key == key,
            new FindOneAndDeleteOptions<MongoRecord, ValueRecord>
            {
                Projection = new FindExpressionProjectionDefinition<MongoRecord, ValueRecord>(x =>
                    new ValueRecord(x.ValueContent, new Audit(x.Details, x.Version)))
            },
            token);

        if (deleted != null)
            logger.LogDebug("MongoDB({@Key}, {Version}) finding: succeeded.", key, deleted.Audit.Version);
        else
            logger.LogDebug("MongoDB({@Key}) finding: not found.", key);

        return deleted.AsOption();
    }
}
