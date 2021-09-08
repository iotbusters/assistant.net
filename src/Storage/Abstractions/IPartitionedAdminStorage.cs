using System.Collections.Generic;
using System.Threading;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An extended abstraction over partitioned storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
    ///     that exposes all stored keys.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    public interface IPartitionedAdminStorage<TKey, TValue> : IPartitionedStorage<TKey, TValue>
    {
        /// <summary>
        ///     Gets all keys in the storage.
        /// </summary>
        IAsyncEnumerable<TKey> GetKeys(CancellationToken token = default);
    }
}