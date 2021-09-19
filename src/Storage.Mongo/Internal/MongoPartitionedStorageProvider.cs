using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class MongoPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private const string StorageDatabaseName = "Storage";
        private const string KeyCollectionName = "PartitionKeys";
        private const string KeyValueCollectionName = "PartitionIndexes";
        private const string ValueCollectionName = "PartitionRecords";

        private readonly IMongoCollection<MongoPartitionKeyRecord> keyCollection;
        private readonly IMongoCollection<MongoPartitionKeyValueRecord> keyValueCollection;
        private readonly IMongoCollection<MongoPartitionValueRecord> valueCollection;
        private readonly ISystemClock clock;

        public MongoPartitionedStorageProvider(
            IMongoClient client,
            ISystemClock clock)
        {
            keyCollection = client.GetDatabase(StorageDatabaseName).GetCollection<MongoPartitionKeyRecord>(KeyCollectionName);
            keyValueCollection = client.GetDatabase(StorageDatabaseName).GetCollection<MongoPartitionKeyValueRecord>(KeyValueCollectionName);
            valueCollection = client.GetDatabase(StorageDatabaseName).GetCollection<MongoPartitionValueRecord>(ValueCollectionName);
            this.clock = clock;
        }

        public async Task<long> Add(KeyRecord key, ValueRecord value, CancellationToken token)
        {
            var valueRecord = new MongoPartitionValueRecord(
                Id: Guid.NewGuid(),
                value.Type,
                value.Content,
                Audit.Initial(created: clock.UtcNow));
            await InsertOne(valueCollection, valueRecord, token);

            // todo: replace infinite loop
            while (true)
            {
                var latestIndex = await keyValueCollection.AsQueryable(new AggregateOptions())
                    .Where(x => x.Key.Id == key.Id)
                    .OrderByDescending(x => x.Key.Index)
                    .Select(x => x.Key.Index)
                    .FirstOrDefaultAsync(token);
                
                if (latestIndex == 0)
                {
                    var initialKeyRecord = new MongoPartitionKeyRecord(
                        new PartitionKey(key.Id, latestIndex),
                        key.Type,
                        key.Content,
                        Audit.Initial(clock.UtcNow));
                    await InsertOne(keyCollection, initialKeyRecord, token);
                }

                var keyRecord = new MongoPartitionKeyValueRecord(new(key.Id, latestIndex + 1), valueRecord.Id);
                if(await InsertOne(keyValueCollection, keyRecord, token))
                    return keyRecord.Key.Index;

                await Task.Delay(10, token);
            }
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken token)
        {
            if (index <= 0)
                throw new ArgumentOutOfRangeException(nameof(index), $"Value must be bigger than 0 but it was {index}.");

            var valueRecord = await
                (from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                 join v in valueCollection on kv.ValueId equals v.Id
                 where kv.Key.Id == key.Id && kv.Key.Index == index
                 select new ValueRecord(v.Type, v.Content, v.Audit)).FirstOrDefaultAsync(token);

            return valueRecord.AsOption();
        }

        public async Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token)
        {
            var requestedKeyValueIds = await
                (from kv in keyValueCollection.AsQueryable(new AggregateOptions())
                    where kv.Key.Id == key.Id && kv.Key.Index > 0 && kv.Key.Index <= upToIndex
                    select kv.Key).ToListAsync(token);

            var deletedCount = await DeleteMany(keyValueCollection, x => requestedKeyValueIds.Contains(x.Key), token);

            var unreferencedKeyIds = await
                (from k in keyCollection.AsQueryable(new AggregateOptions())
                 join kv in keyValueCollection on k.Key equals kv.Key into joined
                 where !joined.Any()
                 select k.Key).ToListAsync(token);

            if (unreferencedKeyIds.Any())
                await DeleteMany(keyCollection, x => unreferencedKeyIds.Contains(x.Key), token);

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
            keyCollection.AsQueryable(new AggregateOptions())
                .Where(x => x.Key.Index == 0)
                .Select(x => new KeyRecord(x.Key.Id, x.Type, x.Content, x.Audit));

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<bool> InsertOne<T>(
            IMongoCollection<T> collection,
            T newRecord,
            CancellationToken token)
        {
            try
            {
                await collection.InsertOneAsync(
                    newRecord,
                    new InsertOneOptions(),
                    token);
                return true;
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // todo: warn error
                return false;
            }
        }

        private async Task<long> DeleteMany<T>(
            IMongoCollection<T> collection,
            Expression<Func<T, bool>> filter,
            CancellationToken token)
        {
            var deleted = await collection.DeleteManyAsync(
                filter,
                new DeleteOptions(),
                token);

            return deleted.DeletedCount;
        }
    }
}
