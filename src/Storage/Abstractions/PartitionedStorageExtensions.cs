using System.Collections.Generic;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Abstractions
{
    public static class PartitionedStorageExtensions
    {
        public static async IAsyncEnumerable<TValue> GetPartition<TKey, TValue>(this IPartitionedStorage<TKey, TValue> storeage, TKey key, long startIndex = 0)
        {
            var index = startIndex;
            while (await storeage.TryGet(key, index++) is Some<TValue>(var value))
                yield return value;
        }
    }
}