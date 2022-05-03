using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    /// <summary>
    ///     <see cref="IHistoricalStorageProvider{TValue}"/> based partitioned storage implementation.
    /// </summary>
    public class GenericPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private readonly IHistoricalStorageProvider<TValue> provider;

        /// <summary/>
        public GenericPartitionedStorageProvider(IHistoricalStorageProvider<TValue> provider) =>
            this.provider = provider;

        /// <inheritdoc/>
        public Task<long> Add(KeyRecord key, ValueRecord value, CancellationToken token) =>
            provider.AddOrUpdate(key, value, token).MapCompleted(x => x.Audit.Version);

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException" />
        public Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken token) =>
            provider.TryGet(key, version: index, token);

        /// <inheritdoc/>
        public Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token) =>
            provider.TryRemove(key, upToVersion: upToIndex, token);

        /// <inheritdoc/>
        public IQueryable<KeyRecord> GetKeys() =>
            provider.GetKeys();

        /// <inheritdoc/>
        public void Dispose() => provider.Dispose();
    }
}
