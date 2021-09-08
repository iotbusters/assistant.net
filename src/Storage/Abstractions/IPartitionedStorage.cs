using Assistant.Net.Unions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction over partitioned storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    public interface IPartitionedStorage<TKey, TValue>
    {
        /// <summary>
        ///    Tries to add a value to partition associated to the <paramref name="key"/> if it doesn't exist.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="value">A value object.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>An added or existed value.</returns>
        Task<long> Add(TKey key, TValue value, CancellationToken token = default);

        /// <summary>
        ///     Tries to find a value in partition associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="index">An index of specific object under the <paramref name="key"/>.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>An existed value if it was found in partition.</returns>
        Task<Option<TValue>> TryGet(TKey key, long index, CancellationToken token = default);
    }
}