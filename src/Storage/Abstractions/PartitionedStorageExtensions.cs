using Assistant.Net.Unions;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     Partitioned storage extensions.
    /// </summary>
    public static class PartitionedStorageExtensions
    {
        /// <summary>
        ///     Gets values from partition associated to the <paramref name="key"/> starting from <paramref name="startIndex"/>.
        /// </summary>
        /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
        /// <typeparam name="TValue">A value object type is stored.</typeparam>
        /// <param name="storage">Original partitioned storage.</param>
        /// <param name="key">A key object.</param>
        /// <param name="startIndex">A partition index.</param>
        /// <returns>A sequence of values in partition.</returns>
        public static async IAsyncEnumerable<TValue> GetPartition<TKey, TValue>(this IPartitionedStorage<TKey, TValue> storage, TKey key, long startIndex = 0)
        {
            var index = startIndex;
            while (await storage.TryGet(key, index++) is Some<TValue>(var value))
                yield return value;
        }
    }
}