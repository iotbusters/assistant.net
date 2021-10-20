using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    An abstraction over internal key based value-centric binary storage provider.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
    public interface IHistoricalStorageProvider<TValue> : IStorageProvider<TValue>, IDisposable
    {
        /// <summary>
        ///     Tries to find a value associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="version">A specific value version.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>An existed value if it was found.</returns>
        Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token = default);

        /// <summary>
        ///     Tries to remove historical value versions associated to the <paramref name="key"/> up to <paramref name="upToVersion"/> (including).
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="upToVersion">A specific value version.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A number of removed value versions.</returns>
        Task<long> TryRemove(KeyRecord key, long upToVersion, CancellationToken token = default);
    }
}