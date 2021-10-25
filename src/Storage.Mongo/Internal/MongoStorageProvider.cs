using Assistant.Net.Abstractions;
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

namespace Assistant.Net.Storage.Internal
{
    internal class MongoStorageProvider<TValue> : IStorageProvider<TValue>
    {
        private readonly ILogger logger;
        private readonly MongoStoringOptions options;
        private readonly IMongoCollection<MongoRecord> collection;
        private readonly ISystemClock clock;

        public MongoStorageProvider(
            ILogger<MongoStorageProvider<TValue>> logger,
            IOptions<MongoStoringOptions> options,
            IMongoClientFactory clientFactory,
            ISystemClock clock)
        {
            this.logger = logger;
            this.options = options.Value;
            this.collection = clientFactory.Create().GetDatabase(this.options.DatabaseName).GetCollection<MongoRecord>(this.options.SingleCollectionName);
            this.clock = clock;
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
                logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

                if (await FindOne(key, token) is Some<ValueRecord>(var found))
                    return found;

                if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
                    return inserted;

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

                if (await FindOneAndReplace(key, updateFactory, token) is Some<ValueRecord>(var replaced))
                    return replaced;

                if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
                    return inserted;

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

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) => FindOne(key, token);

        public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) => DeleteOne(key, token);

        public IQueryable<KeyRecord> GetKeys() =>
            collection.AsQueryable(new AggregateOptions()).Select(x => new KeyRecord(x.Id, x.KeyType, x.KeyContent));

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<Option<ValueRecord>> FindOne(KeyRecord key, CancellationToken token)
        {
            logger.LogDebug("MongoDB({CollectionName}:{RecordId}) finding: begins.", collection.CollectionNamespace.FullName, key.Id);

            var found = await collection.Find(filter: x => x.Id == key.Id, new FindOptions()).SingleOrDefaultAsync(token);

            logger.LogDebug(found != null
                    ? "MongoDB({CollectionName}:{RecordId}) finding: succeeded."
                    : "MongoDB({CollectionName}:{RecordId}) finding: not found.",
                collection.CollectionNamespace.FullName, key.Id);

            return found.AsOption().MapOption(x => new ValueRecord(x.ValueType, x.ValueContent, new Audit(x.Details, x.Version)));
        }

        private async Task<Option<ValueRecord>> InsertOne(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token)
        {
            var added = await addFactory(key);
            added.Audit.Created = clock.UtcNow;
            var addedRecord = new MongoRecord(
                key.Id,
                key.Type,
                key.Content,
                version: 1,
                added.Type,
                added.Content,
                added.Audit.Details);

            logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting: begins.", collection.CollectionNamespace.FullName, key.Id);

            try
            {
                await collection.InsertOneAsync(
                    addedRecord,
                    new InsertOneOptions(),
                    token);

                logger.LogDebug("MongoDB({CollectionName}:{RecordId}) inserting : succeeded.", collection.CollectionNamespace.FullName, key.Id);
                return Option.Some(added with {Audit = new Audit(added.Audit.Details, addedRecord.Version)});
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                logger.LogWarning(ex, "MongoDB({CollectionName}:{RecordId}) inserting: already present.", collection.CollectionNamespace.FullName, key.Id);
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
            updated.Audit.Created = clock.UtcNow;
            var updatedRecord = new MongoRecord(
                key.Id,
                key.Type,
                key.Content,
                version: found.Audit.Version + 1,
                updated.Type,
                updated.Content,
                updated.Audit.Details);

            logger.LogDebug("MongoDB({CollectionName}:{RecordId}:{OldVersion}/{NewVersion}) replacing: begins.",
                collection.CollectionNamespace.FullName, key.Id, found.Audit.Version, updatedRecord.Version);

            var result = await collection.ReplaceOneAsync(
                filter: x => x.Id == key.Id && x.Version == found.Audit.Version,
                updatedRecord,
                new ReplaceOptions(),
                token);

            if (result.MatchedCount != 0)
            {
                logger.LogDebug("MongoDB({CollectionName}:{RecordId}:{OldVersion}/{NewVersion}) replacing: succeeded.",
                    collection.CollectionNamespace.FullName, key.Id, found.Audit.Version, updatedRecord.Version);
                return Option.Some(updated with {Audit = new Audit(updated.Audit.Details, updatedRecord.Version)});
            }

            logger.LogDebug("MongoDB({CollectionName}:{RecordId}:{OldVersion}/{NewVersion}) replacing: outdated version.",
                collection.CollectionNamespace.FullName, key.Id, found.Audit.Version, updatedRecord.Version);
            return Option.None;

        }

        private async Task<Option<ValueRecord>> DeleteOne(KeyRecord key, CancellationToken token)
        {
            logger.LogDebug("MongoDB({CollectionName}:{RecordId}) deleting: begins.", collection.CollectionNamespace.FullName, key.Id);

            var deleted = await collection.FindOneAndDeleteAsync<MongoRecord>(
                filter: x => x.Id == key.Id,
                new FindOneAndDeleteOptions<MongoRecord>(),
                token);

            logger.LogDebug(deleted != null
                    ? "MongoDB({CollectionName}:{RecordId}) deleting: succeeded."
                    : "MongoDB({CollectionName}:{RecordId}) deleting: not found.",
                collection.CollectionNamespace.FullName, key.Id);

            return deleted.AsOption().MapOption(x => new ValueRecord(x.ValueType, x.ValueContent, new Audit(x.Details, x.Version)));
        }
    }
}
