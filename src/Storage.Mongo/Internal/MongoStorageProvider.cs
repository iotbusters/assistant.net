using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
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
        private const string StorageCollectionName = "Records";

        private readonly IMongoCollection<MongoRecord> collection;
        private readonly ISystemClock clock;
        private readonly MongoOptions options;

        public MongoStorageProvider(IOptions<MongoOptions> options, IMongoClient client, ISystemClock clock)
        {
            this.options = options.Value;
            this.collection = client.GetDatabase(this.options.DatabaseName).GetCollection<MongoRecord>(StorageCollectionName);
            this.clock = clock;
        }

        public async Task<ValueRecord> AddOrGet(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            CancellationToken token)
        {
            for (var attempt = 1; attempt <= options.MaxInsertAttemptNumber; attempt++)
            {
                if (await FindOne(key, token) is Some<ValueRecord>(var found))
                    return found;

                if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
                    return inserted;

                await Task.Delay(options.OptimisticConcurrencyDelayTime, token);
            }

            throw new StorageConcurrencyException();
        }

        public async Task<ValueRecord> AddOrUpdate(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken token)
        {
            for (var attempt = 1; attempt <= options.MaxUpsertAttemptNumber; attempt++)
            {
                if (await FindOne(key, token) is Some<ValueRecord>(var found))
                {
                    if (await ReplaceOne(key, found, updateFactory, token) is Some<ValueRecord>(var replaced))
                        return replaced;
                }
                else if (await InsertOne(key, addFactory, token) is Some<ValueRecord>(var inserted))
                    return inserted;

                await Task.Delay(options.OptimisticConcurrencyDelayTime, token);
            }

            throw new StorageConcurrencyException();
        }

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) => FindOne(key, token);

        public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) => await DeleteOne(key, token);

        public IQueryable<KeyRecord> GetKeys() =>
            collection.AsQueryable(new AggregateOptions()).Select(x => new KeyRecord(x.Id, x.KeyType, x.KeyContent, x.Audit));

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<Option<ValueRecord>> FindOne(KeyRecord key, CancellationToken token)
        {
            var existed = await collection.Find(filter: x => x.Id == key.Id, new FindOptions()).SingleOrDefaultAsync(token);
            return existed.AsOption().MapOption(x => new ValueRecord(x.ValueType, x.ValueContent, x.Audit));
        }

        private async Task<Option<ValueRecord>> InsertOne(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token)
        {
            var value = await addFactory(key);
            var audit = Audit.Initial(created: clock.UtcNow);
            var newRecord = new MongoRecord(key.Id, key.Type, key.Content, value.Type, value.Content, audit);

            try
            {
                await collection.InsertOneAsync(
                    newRecord,
                    new InsertOneOptions(),
                    token);
                return Option.Some(value with {Audit = audit});
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // todo: warn error
                return Option.None;
            }
        }

        private async Task<Option<ValueRecord>> ReplaceOne(
            KeyRecord key,
            ValueRecord record,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken token)
        {
            var updatedValue = await updateFactory(key, record);
            var updatedAudit = record.Audit!.IncrementVersion(updated: clock.UtcNow);
            var updatedRecord = new MongoRecord(key.Id, key.Type, key.Content, updatedValue.Type, updatedValue.Content, updatedAudit);

            var result = await collection.ReplaceOneAsync(
                filter: x => x.Id == key.Id && x.Audit.Version == record.Audit.Version,
                updatedRecord,
                new ReplaceOptions(),
                token);

            if (result.MatchedCount == 0)
                return Option.None;

            return Option.Some(updatedValue with {Audit = updatedAudit});

        }

        private async Task<Option<ValueRecord>> DeleteOne(KeyRecord key, CancellationToken token)
        {
            var deleted = await collection.FindOneAndDeleteAsync<MongoRecord>(
                filter: x => x.Id == key.Id,
                new FindOneAndDeleteOptions<MongoRecord>(),
                token);
            return deleted.AsOption().MapOption(x => new ValueRecord(x.ValueType, x.ValueContent, x.Audit));
        }
    }
}
