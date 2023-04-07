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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class MongoHistoricalStorageProvider<TValue> : IHistoricalStorageProvider<TValue>
{
    private readonly ILogger logger;
    private readonly MongoStoringOptions options;
    private readonly IMongoCollection<MongoVersionedRecord> collection;

    public MongoHistoricalStorageProvider(
        ILogger<MongoHistoricalStorageProvider<TValue>> logger,
        INamedOptions<MongoStoringOptions> options,
        IMongoDatabase database)
    {
        this.logger = logger;
        this.options = options.Value;
        this.collection = database.GetCollection<MongoVersionedRecord>(MongoNames.HistoricalStorageCollectionName);
    }

    public async Task<ValueRecord> AddOrGet(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        CancellationToken token)
    {
        var strategy = options.UpsertRetry;
        var keyId = new Key(key.Id, key.ValueType);

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({@Key}): #{Attempt} begins.", keyId, attempt);

            if (await FindLatestOne(keyId, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): #{Attempt} found.",
                    keyId, found.Audit.Version, attempt);
                return found;
            }

            var added = await addFactory(key);
            var inserted = new MongoVersionedRecord(
                new(keyId, added.Audit.Version),
                key.Type,
                key.Content,
                added.Content,
                added.Audit.Details);
            if (await InsertOne(inserted, token))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): #{Attempt} added.",
                    keyId, added.Audit.Version, attempt);
                return added;
            }

            logger.LogWarning("Storage.AddOrGet({@Key}): #{Attempt} failed.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({@Key}): #{Attempt} reached the limit.", keyId, attempt);
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
            logger.LogInformation("Storage.AddOrUpdate({@Key}): #{Attempt} begins.", keyId, attempt);

            var value = await FindLatestOne(keyId, token) is not Some<ValueRecord>(var found)
                ? await addFactory(key)
                : await updateFactory(key, found);

            var record = new MongoVersionedRecord(
                new(keyId, value.Audit.Version),
                key.Type,
                key.Content,
                value.Content,
                value.Audit.Details);
            if (await InsertOne(record, token))
            {
                logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): #{Attempt} added version.",
                    keyId, value.Audit.Version, attempt);
                return value;
            }

            logger.LogWarning("Storage.AddOrUpdate({@Key}): #{Attempt} failed.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({@Key}): #{Attempt} won't proceed.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryGet({@Key}): begins.", keyId);

        if (await FindLatestOne(keyId, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogInformation("Storage.TryGet({@Key}, {Version}): found.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogInformation("Storage.TryGet({@Key}): not found.", keyId);
        return Option.None;
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryGet({@Key}, {Version}): begins.", keyId, version);

        if (await FindExactOne(keyId, version, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogInformation("Storage.TryGet({@Key}, {Version}): found.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogInformation("Storage.TryGet({@Key}, {Version}): not found.", keyId, version);
        return Option.None;
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var strategy = options.DeleteRetry;
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryRemove({@Key}, 1..latest): begins.", keyId);

        ValueRecord? latestValue = null;
        var attempt = 1;
        while (true)
        {
            if (await FindLatestOne(keyId, token) is not Some<ValueRecord>(var found))
                break;

            if (latestValue != null && found.Audit.Version < latestValue.Audit.Version)
                break; // note: found new value version sequence was started.

            latestValue = found;

            await DeleteMany(keyId, found.Audit.Version, token);

            logger.LogWarning("Storage.TryRemove({@Key}, 1..latest): #{Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({@Key}, 1..latest): #{Attempt} reached the limit.", keyId, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        if (latestValue != null)
        {
            logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): ends.", keyId, latestValue.Audit.Version);
            return latestValue.AsOption();
        }

        logger.LogInformation("Storage.TryRemove({@Key}, 1..latest): not found.", keyId);
        return Option.None; // note: value doesn't exist.
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, long upToVersion, CancellationToken token)
    {
        if (upToVersion <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.");

        var strategy = options.DeleteRetry;
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): begins.", keyId, upToVersion);

        ValueRecord? latestValue = null;
        var attempt = 1;
        while (true)
        {
            if (await FindLatestOne(keyId, upToVersion, token) is not Some<ValueRecord>(var found))
                break;

            if (latestValue != null && found.Audit.Version <= latestValue.Audit.Version)
                break; // note: found new value version sequence was started.

            latestValue = found;

            await DeleteMany(keyId, found.Audit.Version, token);

            logger.LogWarning("Storage.TryRemove({@Key}, 1..{Version}): #{Attempt} ends.", keyId, upToVersion, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({@Key}, 1..{Version}): #{Attempt} reached the limit.",
                    keyId, upToVersion, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        if (latestValue != null)
        {
            logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): ends.", keyId, latestValue.Audit.Version);
            return Option.Some(latestValue);
        }

        logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): not found.", keyId, upToVersion);
        return Option.None; // note: value doesn't exist.
    }

    public async IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, [EnumeratorCancellation] CancellationToken token)
    {
        logger.LogInformation("Storage.GetKeys(): begins.");

        using var cursor = await collection.AsQueryable(new())
            .Select(x => new KeyRecord(x.Key.Key.Id, x.KeyType, x.Key.Key.ValueType, x.KeyContent))
            .Where(predicate)
            .Distinct()
            .ToCursorAsync(token);
        while (await cursor.MoveNextAsync(token))
            foreach (var key in cursor.Current)
                yield return key;

        logger.LogInformation("Storage.GetKeys(): ends.");
    }

    private async Task<Option<ValueRecord>> FindLatestOne(Key key, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, 1..latest) finding: begins.", key);

        var found = await collection.Find(x => x.Key.Key == key, new())
            .Limit(1)
            .Sort(new SortDefinitionBuilder<MongoVersionedRecord>().Descending(x => x.Key.Version))
            .SingleOrDefaultAsync(token);

        if (found != null)
        {
            logger.LogDebug("MongoDB({@Key}, 1..latest) finding: ends.", found.Key);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("MongoDB({@Key}, 1..latest) finding: not found.", key);
        return Option.None;
    }

    private async Task<Option<ValueRecord>> FindLatestOne(Key key, long upToVersion, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, 1..{Version}) finding: begins.", key, upToVersion);

        var found = await collection.Find(x => x.Key.Key == key && x.Key.Version <= upToVersion, new())
            .Sort(new SortDefinitionBuilder<MongoVersionedRecord>().Descending(x => x.Key.Version))
            .Limit(1)
            .SingleOrDefaultAsync(token);

        if (found != null)
        {
            logger.LogDebug("MongoDB({@Key}, {Version}) finding: ends.", found.Key, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("MongoDB({@Key}, 1..{Version}) finding: not found.", key, upToVersion);
        return Option.None;
    }

    private async Task<Option<ValueRecord>> FindExactOne(Key key, long version, CancellationToken token)
    {
        var keyVersion = new KeyVersion(key, version);

        logger.LogDebug("MongoDB({@Key}, {Version}) finding: begins.", key, version);

        var found = await collection.Find(x => x.Key == keyVersion, new())
            .Limit(1)
            .SingleOrDefaultAsync(token);

        if (found != null)
        {
            logger.LogDebug("MongoDB({@Key}, {Version}) finding: ends.", key, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("MongoDB({@Key}, {Version}) finding: not found.", key, version);
        return Option.None;
    }

    private static ValueRecord ToValue(MongoVersionedRecord record) =>
        new(record.ValueContent, new(record.Details, record.Key.Version));

    private async Task<bool> InsertOne(MongoVersionedRecord record, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, {Version}) inserting: begins.", record.Key.Key, record.Key.Version);
        try
        {
            await collection.InsertOneAsync(
                record,
                new(),
                token);
            logger.LogDebug("MongoDB({@Key}, {Version}) inserting: ends.", record.Key.Key, record.Key.Version);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            logger.LogWarning(ex, "MongoDB({@Key}, {Version}) inserting: already present.", record.Key.Key, record.Key.Version);
            return false;
        }
    }

    private async Task DeleteMany(Key key, long upToVersion, CancellationToken token)
    {
        logger.LogDebug("MongoDB({@Key}, 1..{Version}) deleting: begins.", key, upToVersion);

        var deleted = await collection.DeleteManyAsync(x =>
            x.Key.Key == key && x.Key.Version <= upToVersion, new(), token);

        logger.LogDebug("MongoDB({@Key}, 1..{Version}) deleting: ends with {DeletedCount}.",
            key, upToVersion, deleted.DeletedCount);
    }
}
