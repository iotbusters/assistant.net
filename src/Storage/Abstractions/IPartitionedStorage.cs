using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Abstractions
{
    public interface IPartitionedStorage<TKey, TValue>
    {
        Task<long> Add(TKey key, TValue value);
        Task<Option<TValue>> TryGet(TKey key, long index);
        IAsyncEnumerable<TKey> GetKeys();
    }
}