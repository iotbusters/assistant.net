using Assistant.Net.Storage.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    An abstraction over internal key based value-centric binary partitioned storage provider.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific partitioned storage implementation is assigned to.</typeparam>
    public interface IPartitionedStorageProvider<TValue> : IPartitionedStorage<KeyRecord, ValueRecord>, IDisposable
    {
        /// <summary>
        ///     Gets all keys in the storage.
        /// </summary>
        IQueryable<KeyRecord> GetKeys();

        /// <summary>
        ///     Tries to remove values below the <paramref name="upToIndex"/> inclusive associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A partition key object.</param>
        /// <param name="upToIndex">An index of specific object under the <paramref name="key"/>.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>Actually removed number of values.</returns>
        Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token = default);
    }
}
