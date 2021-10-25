using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class MongoHistoricalStorageProvider<TValue> : IHistoricalStorageProvider<TValue>
    {
        private readonly ILogger logger;
        private readonly MongoStoringOptions options;
        private readonly IMongoCollection<MongoKeyRecord> keyCollection;
        private readonly IMongoCollection<MongoKeyValueRecord> keyValueCollection;
        private readonly IMongoCollection<MongoValueRecord> valueCollection;
        private readonly ISystemClock clock;

        public MongoHistoricalStorageProvider(
            ILogger<MongoHistoricalStorageProvider<TValue>> logger,
            IOptions<MongoStoringOptions> options,
            IMongoClientFactory clientFactory,
            ISystemClock clock)
        {
            this.logger = logger;
            this.options = options.Value;
            this.clock = clock;

            var database = clientFactory.Create().GetDatabase(this.options.DatabaseName);
            this.keyCollection = database.GetCollection<MongoKeyRecord>(this.options.KeyCollectionName);
            this.keyValueCollection = database.GetCollection<MongoKeyValueRecord>(this.options.KeyValueCollectionName);
            this.valueCollection = database.GetCollection<MongoValueRecord>(this.options.ValueCollectionName);
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
                logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

                if (await TryGet(key, token) is Some<ValueRecord>(var currentValue))
                {
                    logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} got value.", key.Id, attempt);
                    return currentValue;
                }

                var addedKey = new MongoKeyRecord(key.Id, key.Type, key.Content);
                await InsertOne(keyCollection, addedKey, token);

                var newValue = await addFactory(key);
                if (await AddValue(key, currentValue: null, newValue, token) is Some<ValueRecord>(var added))
                {
                    logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} added initial version.", key.Id, attempt);
                    return added;
                }

                attempt++;
                if (!strategy.CanRetry(attempt))
                {
                    logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                    break;
                }

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
                logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} begins.", key.Id, attempt);

                ValueRecord newValue;
                if (await TryGet(key, token) is not Some<ValueRecord>(var currentValue))
                {
                    var addedKey = new MongoKeyRecord(key.Id, key.Type, key.Content);
                    await InsertOne(keyCollection, addedKey, token);

                    currentValue = null;
                    newValue = await addFactory(key);
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} adding value.", key.Id, attempt);
                }
                else
                {
                    newValue = await updateFactory(key, currentValue);
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} updating value.", key.Id, attempt);
                }

                if (await AddValue(key, currentValue, newValue, token) is Some<ValueRecord>(var added))
                {
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} added new version.", key.Id, attempt);
                    return added;
                }

                attempt++;
                if (!strategy.CanRetry(attempt))
                {
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                    break;
                }

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            throw new StorageConcurrencyException();
        }

        public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
        {
            logger.LogDebug("Storage.TryGet({KeyId}): begins.", key.Id);

            var currentValue = await (
                    from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                    where kv.Key.Id == key.Id
                    orderby kv.Key.Version descending
                    join v in valueCollection on kv.ValueId equals v.Id
                    select new ValueRecord(v.Type, v.Content, new Audit(v.Details, kv.Key.Version)))
                .FirstOrDefaultAsync(token);

            if (currentValue == null)
            {
                logger.LogDebug("Storage.TryGet({KeyId}): not found.", key.Id);
                return Option.None; // note: key doesn't exist.
            }

            logger.LogDebug("Storage.TryGet({KeyId}): found.", key.Id);
            return Option.Some(currentValue);
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token)
        {
            if (version <= 0)
                throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {version}.");

            logger.LogDebug("Storage.TryGet({KeyId}:{Version}): begins.", key.Id, version);

            var currentValue = await (
                    from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                    where kv.Key.Id == key.Id && kv.Key.Version == version
                    join v in valueCollection on kv.ValueId equals v.Id
                    select new ValueRecord(v.Type, v.Content, new Audit(v.Details, kv.Key.Version)))
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

            logger.LogDebug("Storage.TryRemove({KeyId}:*/*): cleaning key-value references.", key.Id);

            ValueRecord? currentValue = null;
            var attempt = 1;
            while (true)
            {
                logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} begins.", key.Id, attempt);

                if (await TryGet(key, token) is not Some<ValueRecord>(var found))
                    break; // note: no value versions.
                if(currentValue != null && found.Audit.Version < currentValue.Audit.Version)
                    break; // note: found new value version sequence was started.

                currentValue = found;
                await DeleteMany(
                    keyValueCollection,
                    filter: x => x.Key.Id == key.Id && x.Key.Version <= currentValue.Audit.Version,
                    token);

                attempt++;
                if (!strategy.CanRetry(attempt))
                {
                    logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                    break;
                }

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            if (currentValue == null)
            {
                logger.LogDebug("Storage.TryRemove({KeyId}): not found.", key.Id);
                return Option.None; // note: key doesn't exist.
            }

            logger.LogDebug("Storage.TryRemove({KeyId}): cleaning unreferenced keys.", key.Id);
            var unreferencedKeyIds = await
                (from k in keyCollection.AsQueryable(new AggregateOptions())
                    join kv in keyValueCollection on k.Id equals kv.Key.Id into joined
                    where !joined.Any()
                    select k.Id).ToListAsync(token);
            if (unreferencedKeyIds.Any())
                // note: remove unreferenced keys left after previous operations.
                await DeleteMany(keyCollection, x => unreferencedKeyIds.Contains(x.Id), token);

            logger.LogDebug("Storage.TryRemove({KeyId}): cleaning unreferenced value versions.", key.Id);
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
            if (upToVersion <= 0)
                throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.");

            logger.LogDebug("Storage.TryRemove({KeyId}:*/{Version}): cleaning key-value references.", key.Id, upToVersion);

            var deletedCount = await DeleteMany(
                keyValueCollection,
                filter: x => x.Key.Id == key.Id && x.Key.Version <= upToVersion,
                token);

            logger.LogDebug("Storage.TryRemove({KeyId}): cleaning unreferenced value versions.", key.Id);
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
            join kv in keyValueCollection on k.Id equals kv.Key.Id into kvs
            where kvs.Any()
            select new KeyRecord(k.Id, k.Type, k.Content);

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<Option<ValueRecord>> AddValue(
            KeyRecord key,
            ValueRecord? currentValue,
            ValueRecord newValue,
            CancellationToken token)
        {
            var oldVersion = currentValue?.Audit.Version ?? 0;
            var newVersion = oldVersion + 1;
            var addedValue = newValue with {Audit = new Audit(newValue.Audit.Details, newVersion) {Created = clock.UtcNow}};

            var valueRecord = new MongoValueRecord(
                Id: Guid.NewGuid().ToString(),
                addedValue.Type,
                addedValue.Content,
                addedValue.Audit.Details);
            if (!await InsertOne(valueCollection, valueRecord, token))
                return Option.None; // note: not really expected issue.

            var keyValueRecord = new MongoKeyValueRecord(new(key.Id, newVersion), valueRecord.Id);
            var isInserted = await InsertOne(keyValueCollection, keyValueRecord, token);

            if (!isInserted)
                return Option.None; // note: the value version is added.

            return Option.Some(addedValue);
        }

        private async Task<bool> InsertOne<T>(IMongoCollection<T> collection, T record, CancellationToken token)
            where T : IRecordIdentity
        {
            logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting: begins.", collection.CollectionNamespace.FullName, record.Id);
            try
            {
                await collection.InsertOneAsync(
                    record,
                    new InsertOneOptions(),
                    token);
                logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting : succeeded.", collection.CollectionNamespace.FullName, record.Id);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                logger.LogWarning(ex, "MongoDB({CollectionName}:{RecordId}) inserting: already present.", collection.CollectionNamespace.FullName, record.Id);
                return false;
            }
        }

        private async Task<long> DeleteMany<T>(IMongoCollection<T> collection, Expression<Func<T, bool>> filter, CancellationToken token)
        {
            logger.LogDebug("MongoDB({CollectionName}) deleting: begins.", collection.CollectionNamespace.FullName);

            var deleted = await collection.DeleteManyAsync(filter, new DeleteOptions(), token);

            logger.LogDebug("MongoDB({CollectionName}) deleting: found {Records}.", collection.CollectionNamespace.FullName, deleted.DeletedCount);

            return deleted.DeletedCount;
        }
    }
}
