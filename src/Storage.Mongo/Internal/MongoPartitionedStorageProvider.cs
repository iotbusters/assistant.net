using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
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
    internal class MongoPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private const int DefaultPartitionIndex = 0;

        private readonly MongoStoringOptions options;
        private readonly ISystemClock clock;
        private readonly IMongoCollection<MongoPartitionKeyRecord> keyCollection;
        private readonly IMongoCollection<MongoPartitionKeyValueRecord> keyValueCollection;
        private readonly IMongoCollection<MongoPartitionValueRecord> valueCollection;

        public MongoPartitionedStorageProvider(IOptions<MongoStoringOptions> options, IMongoClient client, ISystemClock clock)
        {
            this.options = options.Value;
            this.clock = clock;

            var database = client.GetDatabase(this.options.DatabaseName);
            this.keyCollection = database.GetCollection<MongoPartitionKeyRecord>(MongoNames.PartitionStorageKeyCollectionName);
            this.keyValueCollection = database.GetCollection<MongoPartitionKeyValueRecord>(MongoNames.PartitionStorageKeyValueCollectionName);
            this.valueCollection = database.GetCollection<MongoPartitionValueRecord>(MongoNames.PartitionStorageValueCollectionName);
        }

        public async Task<long> Add(KeyRecord key, ValueRecord value, CancellationToken token)
        {
            var strategy = options.InsertRetry;

            var valueRecord = new MongoPartitionValueRecord(
                Id: Guid.NewGuid(),
                value.Type,
                value.Content,
                Audit.Initial(created: clock.UtcNow));
            await TryInsertOne(valueCollection, valueRecord, token);

            var attempt = 1;
            while (true)
            {
                var latestIndex = await keyValueCollection.AsQueryable(new AggregateOptions())
                    .Where(x => x.Key.Id == key.Id)
                    .OrderByDescending(x => x.Key.Index)
                    .Select(x => x.Key.Index)
                    .FirstOrDefaultAsync(token);

                if (latestIndex == DefaultPartitionIndex)
                {
                    var initialKeyRecord = new MongoPartitionKeyRecord(
                        new PartitionKey(key.Id, latestIndex),
                        key.Type,
                        key.Content,
                        Audit.Initial(clock.UtcNow));
                    await TryInsertOne(keyCollection, initialKeyRecord, token);
                }

                var keyRecord = new MongoPartitionKeyValueRecord(new(key.Id, latestIndex + 1), valueRecord.Id);
                if(await TryInsertOne(keyValueCollection, keyRecord, token))
                    return keyRecord.Key.Index;

                attempt++;
                if (!strategy.CanRetry(attempt))
                    break;

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            throw new StorageConcurrencyException();
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken token)
        {
            if (index <= DefaultPartitionIndex)
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
                    where kv.Key.Id == key.Id && kv.Key.Index > DefaultPartitionIndex && kv.Key.Index <= upToIndex
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
                .Where(x => x.Key.Index == DefaultPartitionIndex)
                .Select(x => new KeyRecord(x.Key.Id, x.Type, x.Content, x.Audit));

        public void Dispose() { /* The mongo client is DI managed. */ }

        private async Task<bool> TryInsertOne<T>(IMongoCollection<T> collection, T newRecord, CancellationToken token)
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

        private async Task<long> DeleteMany<T>(IMongoCollection<T> collection, Expression<Func<T, bool>> filter, CancellationToken token)
        {
            var deleted = await collection.DeleteManyAsync(
                filter,
                new DeleteOptions(),
                token);
            return deleted.DeletedCount;
        }
    }
}
