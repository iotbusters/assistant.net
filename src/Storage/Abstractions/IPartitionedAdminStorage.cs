using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     An extended abstraction over partitioned storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
///     that exposes all stored keys.
/// </summary>
/// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
/// <typeparam name="TValue">A value object type is stored.</typeparam>
public interface IPartitionedAdminStorage<TKey, TValue> : IPartitionedStorage<TKey, TValue>
{
    /// <summary>
    ///    Tries to add a detailed value to partition associated to the <paramref name="key"/> if it doesn't exist.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="value">A value object.</param>
    /// <param name="token"/>
    /// <returns>A partition index of added value.</returns>
    Task<PartitionValue<TValue>> Add(TKey key, StorageValue<TValue> value, CancellationToken token = default);

    /// <summary>
    ///     Tries to find a value in partition associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="index">An index of specific object under the <paramref name="key"/>.</param>
    /// <param name="token"/>
    /// <returns>An existed detailed value if it was found in partition.</returns>
    Task<Option<PartitionValue<TValue>>> TryGetDetailed(TKey key, long index, CancellationToken token = default);

    /// <summary>
    ///     Gets all keys in the storage.
    /// </summary>
    IAsyncEnumerable<TKey> GetKeys(CancellationToken token = default);

    /// <summary>
    ///     Tries to remove the whole partition associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="token"/>
    /// <returns>A latest removed value if it was found.</returns>
    Task<Option<TValue>> TryRemove(TKey key, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove partition value versions associated to the <paramref name="key"/>
    ///     up to <paramref name="upToIndex"/> (including).
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="upToIndex">A specific index.</param>
    /// <param name="token"/>
    /// <returns>A latest removed value if it was found.</returns>
    Task<Option<TValue>> TryRemove(TKey key, long upToIndex, CancellationToken token = default);
}
