using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class MongoHistoricalStorageProvider<TValue> : IHistoricalStorageProvider<TValue>
{
    private readonly ILogger logger;
    private readonly MongoStoringOptions options;
    private readonly IMongoCollection<MongoKeyRecord> keyCollection;
    private readonly IMongoCollection<MongoKeyValueRecord> keyValueCollection;
    private readonly IMongoCollection<MongoValueRecord> valueCollection;

    public MongoHistoricalStorageProvider(
        ILogger<MongoHistoricalStorageProvider<TValue>> logger,
        INamedOptions<MongoStoringOptions> options,
        IMongoDatabase database)
    {
        this.logger = logger;
        this.options = options.Value;
        this.keyCollection = database.GetCollection<MongoKeyRecord>(MongoNames.HistoricalStorageKeyCollectionName);
        this.keyValueCollection = database.GetCollection<MongoKeyValueRecord>(MongoNames.HistoricalStorageKeyValueCollectionName);
        this.valueCollection = database.GetCollection<MongoValueRecord>(MongoNames.HistoricalStorageValueCollectionName);
    }

    public async Task<ValueRecord> AddOrGet(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        CancellationToken token)
    {
        var strategy = options.UpsertRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await TryGet(key, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} found.",
                    key.Id, found.Audit.Version, attempt);
                return found;
            }

            var id = new Key(key.Id, key.ValueType);
            var addedKey = new MongoKeyRecord(id, key.Type, key.Content, key.ValueType);
            await InsertOne(keyCollection, addedKey, id, token);

            var newValue = await addFactory(key);
            if (await AddValue(key, newValue, token) is Some<ValueRecord>(var added))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} added.",
                    key.Id, added.Audit.Version, attempt);
                return added;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({KeyId}): {Attempt} reached the limit.", key.Id, attempt);
                break;
            }

            logger.LogWarning("Storage.AddOrGet({KeyId}): {Attempt} failed.", key.Id, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new StorageConcurrencyException();
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

            ValueRecord newValue;
            if (await TryGet(key, token) is not Some<ValueRecord>(var currentValue))
            {
                var id = new Key(key.Id, key.ValueType);
                var addedKey = new MongoKeyRecord(id, key.Type, key.Content, key.ValueType);
                await InsertOne(keyCollection, addedKey, id, token);

                newValue = await addFactory(key);
            }
            else
                newValue = await updateFactory(key, currentValue);

            if (await AddValue(key, newValue, token) is Some<ValueRecord>(var added))
            {
                logger.LogInformation("Storage.AddOrUpdate({KeyId}, {Version}): {Attempt} added.",
                    key.Id, added.Audit.Version, attempt);
                return added;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                break;
            }

            logger.LogWarning("Storage.AddOrUpdate({KeyId}): {Attempt} failed.", key.Id, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new StorageConcurrencyException();
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        logger.LogInformation("Storage.TryGet({KeyId}): begins.", key.Id);

        var currentValue = await (
                from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                where kv.KeyVersion.Key == new Key(key.Id, key.ValueType)
                orderby kv.KeyVersion.Version descending
                join v in valueCollection on kv.ValueId equals v.Id
                select new ValueRecord(v.Type, v.Content, new Audit(v.Details, kv.KeyVersion.Version)))
            .FirstOrDefaultAsync(token);

        if (currentValue == null)
        {
            logger.LogInformation("Storage.TryGet({KeyId}): not found.", key.Id);
            return Option.None; // note: key doesn't exist.
        }

        logger.LogInformation("Storage.TryGet({KeyId}): found.", key.Id);
        return Option.Some(currentValue);
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token)
    {
        logger.LogDebug("Storage.TryGet({KeyId}:{Version}): begins.", key.Id, version);

        var currentValue = await (
                from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                where kv.KeyVersion.Key == new Key(key.Id, key.ValueType) && kv.KeyVersion.Version == version
                join v in valueCollection on kv.ValueId equals v.Id
                select new ValueRecord(v.Type, v.Content, new Audit(v.Details, kv.KeyVersion.Version)))
            .SingleOrDefaultAsync(token);

        if (currentValue == null)
        {
            logger.LogDebug("Storage.TryGet({KeyId}:{Version}): not found.", key.Id, version);
            return Option.None; // note: key doesn't exist.
        }

        logger.LogDebug("Storage.TryGet({KeyId}:{Version}): found.", key.Id, version);
        return Option.Some(currentValue);
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var strategy = options.DeleteRetry;

        logger.LogInformation("Storage.TryRemove({KeyId}:*/*): cleaning key-value references.", key.Id);

        ValueRecord? currentValue = null;
        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.TryRemove({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await TryGet(key, token) is not Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.TryRemove({KeyId}): {Attempt} succeeded.", key.Id, attempt);
                break; // note: no value versions.
            }

            if (currentValue != null && found.Audit.Version < currentValue.Audit.Version)
            {
                logger.LogInformation("Storage.TryRemove({KeyId}): {Attempt} succeeded.", key.Id, attempt);
                break; // note: found new value version sequence was started.
            }

            currentValue = found;
            await DeleteMany(
                keyValueCollection,
                filter: x => x.KeyVersion.Key == new Key(key.Id, key.ValueType) && x.KeyVersion.Version <= currentValue.Audit.Version,
                token);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({KeyId}): {Attempt} reached the limit.", key.Id, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        if (currentValue == null)
        {
            logger.LogInformation("Storage.TryRemove({KeyId}): not found.", key.Id);
            return Option.None; // note: key doesn't exist.
        }

        logger.LogInformation("Storage.TryRemove({KeyId}): cleaning unreferenced keys.", key.Id);
        var unreferencedKeyIds = await
            (from k in keyCollection.AsQueryable(new AggregateOptions())
                join kv in keyValueCollection on k.Key equals kv.KeyVersion.Key into joined
                where !joined.Any()
                select k.Key).ToListAsync(token);
        if (unreferencedKeyIds.Any())
            // note: remove unreferenced keys left after previous operations.
            await DeleteMany(keyCollection, x => unreferencedKeyIds.Contains(x.Key), token);

        logger.LogInformation("Storage.TryRemove({KeyId}): cleaning unreferenced value versions.", key.Id);
        var unreferencedValueIds = await
            (from v in valueCollection.AsQueryable(new AggregateOptions())
                join kv in keyValueCollection on v.Id equals kv.ValueId into joined
                where !joined.Any()
                select v.Id).ToListAsync(token);
        if (unreferencedValueIds.Any())
            // note: remove unreferenced values left after previous operations.
            await DeleteMany(valueCollection, x => unreferencedValueIds.Contains(x.Id), token);

        return currentValue.AsOption();
    }

    public async Task<long> TryRemove(KeyRecord key, long upToVersion, CancellationToken token)
    {
        logger.LogInformation("Storage.TryRemove({KeyId}, 1..{Version}): cleaning key-value references.", key.Id, upToVersion);

        var deletedCount = await DeleteMany(
            keyValueCollection,
            filter: x => x.KeyVersion.Key == new Key(key.Id, key.ValueType) && x.KeyVersion.Version <= upToVersion,
            token);

        logger.LogInformation("Storage.TryRemove({KeyId}): cleaning unreferenced value versions.", key.Id);
        var unreferencedValueIds = await
            (from v in valueCollection.AsQueryable(new AggregateOptions())
                join kv in keyValueCollection on v.Id equals kv.ValueId into joined
                where !joined.Any()
                select v.Id).ToListAsync(token);
        if (unreferencedValueIds.Any())
            await DeleteMany(valueCollection, x => unreferencedValueIds.Contains(x.Id), token);

        return deletedCount;
    }

    public IQueryable<KeyRecord> GetKeys() =>
        from k in keyCollection.AsQueryable(new AggregateOptions())
        join kv in keyValueCollection on k.Key equals kv.KeyVersion.Key into kvs
        where kvs.Any()
        select new KeyRecord(k.Key.Id, k.Type, k.Content, k.Key.ValueType);

    private async Task<Option<ValueRecord>> AddValue(
        KeyRecord key,
        ValueRecord newValue,
        CancellationToken token)
    {
        var valueRecord = new MongoValueRecord(
            Id: Guid.NewGuid().ToString(),
            newValue.Type,
            newValue.Content,
            newValue.Audit.Details);
        if (!await InsertOne(valueCollection, valueRecord, valueRecord.Id, token))
            return Option.None; // note: not really expected issue.

        var keyValueRecord = new MongoKeyValueRecord(new(new(key.Id, key.ValueType), newValue.Audit.Version), valueRecord.Id);
        var isInserted = await InsertOne(keyValueCollection, keyValueRecord, keyValueRecord.KeyVersion, token);

        if (!isInserted)
            return Option.None; // note: the value version is added.

        return Option.Some(newValue);
    }

    private async Task<bool> InsertOne<T>(IMongoCollection<T> collection, T record, object id, CancellationToken token)
    {
        logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting: begins.", collection.CollectionNamespace.FullName, id);
        try
        {
            await collection.InsertOneAsync(
                record,
                new InsertOneOptions(),
                token);
            logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting: succeeded.", collection.CollectionNamespace.FullName, id);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(ex, "MongoDB({CollectionName}:{RecordId}) inserting: already present.", collection.CollectionNamespace.FullName, id);
            return false;
        }
    }

    private async Task<long> DeleteMany<T>(IMongoCollection<T> collection, Expression<Func<T, bool>> filter, CancellationToken token)
    {
        logger.LogDebug("MongoDB({CollectionName}) deleting: begins.", collection.CollectionNamespace.FullName);

        var deleted = await collection.DeleteManyAsync(filter, new DeleteOptions(), token);

        logger.LogDebug("MongoDB({CollectionName}) deleting: found {Count}.", collection.CollectionNamespace.FullName, deleted.DeletedCount);

        return deleted.DeletedCount;
    }
}
