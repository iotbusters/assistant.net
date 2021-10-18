using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class MongoPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private readonly IHistoricalStorageProvider<TValue> provider;

        public MongoPartitionedStorageProvider(IHistoricalStorageProvider<TValue> provider) =>
            this.provider = provider;

        public Task<long> Add(KeyRecord key, ValueRecord value, CancellationToken token) =>
            provider.AddOrUpdate(key, value, token).MapCompleted(x => x.Audit.Version);

        /// <exception cref="ArgumentOutOfRangeException" />
        public Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken token) =>
            provider.TryGet(key, version: index, token);

        public Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token) =>
            provider.TryRemove(key, upToVersion: upToIndex, token);

        public IQueryable<KeyRecord> GetKeys() =>
            provider.GetKeys();

        public void Dispose() => provider.Dispose();
    }
}
