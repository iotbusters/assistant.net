using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class MongoStorageProvider<TValue> : IStorageProvider<TValue>
    {
        private const string StorageDatabaseName = "Storage";
        private const string StorageCollectionName = "Records";

        private readonly IMongoCollection<MongoRecord> collection;
        private readonly ISystemClock clock;

        public MongoStorageProvider(IMongoClient client, ISystemClock clock)
        {
            this.collection = client.GetDatabase(StorageDatabaseName).GetCollection<MongoRecord>(StorageCollectionName);
            this.clock = clock;
        }

        public async Task<ValueRecord> AddOrGet(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            CancellationToken token)
        {
            // todo: replace infinite loop
            while (true)
            {
                if (await FindOne(key, token) is Some<ValueRecord> found)
                    return found.Value;

                if (await InsertOne(key, addFactory, token) is Some<ValueRecord> inserted)
                    return inserted.Value;

                await Task.Delay(10, token);
            }
        }

        public async Task<ValueRecord> AddOrUpdate(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken token)
        {
            // todo: replace infinite loop
            while (true)
            {
                if (await FindOne(key, token) is Some<ValueRecord> found)
                {
                    if (await ReplaceOne(key, found.Value, updateFactory, token) is Some<ValueRecord> replaced)
                        return replaced.Value;
                }
                else if (await InsertOne(key, addFactory, token) is Some<ValueRecord> inserted)
                    return inserted.Value;

                await Task.Delay(10, token);
            }
        }

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) => FindOne(key, token);

        public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) => await DeleteOne(key, token);

        public IQueryable<KeyRecord> GetKeys() =>
            collection.AsQueryable(new AggregateOptions()).Select(x => new KeyRecord(x.Id, x.KeyType, x.KeyContent, x.Audit));

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<Option<ValueRecord>> FindOne(KeyRecord key, CancellationToken token)
        {
            var cursor = await collection.FindAsync(
                filter: x => x.Id == key.Id,
                new FindOptions<MongoRecord>(),
                token);
            var existed = await cursor.FirstOrDefaultAsync(token);
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
            ValueRecord currentValue,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken token)
        {
            var updatedValue = await updateFactory(key, currentValue);
            var updatedAudit = currentValue.Audit!.IncrementVersion(updated: clock.UtcNow);
            var updatedRecord = new MongoRecord(key.Id, key.Type, key.Content, updatedValue.Type, updatedValue.Content, updatedAudit);

            var result = await collection.ReplaceOneAsync(
                filter: x => x.Id == key.Id && x.Audit.Version == currentValue.Audit.Version,
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
