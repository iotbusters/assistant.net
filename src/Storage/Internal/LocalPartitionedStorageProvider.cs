using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Internal
{
    internal class LocalPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private readonly ConcurrentDictionary<StoreKey, ConcurrentDictionary<long, byte[]>> backedStorage = new();

        public Task<long> Add(StoreKey key, byte[] value)
        {
            var partition = backedStorage.GetOrAdd(key, _ => new ConcurrentDictionary<long, byte[]>());

            // ignore chance of blocking.
            while (!partition.TryAdd(partition.Count, value)) ;

            return Task.FromResult((long)partition.Count);
        }

        public Task<Option<byte[]>> TryGet(StoreKey key, long index)
        {
            if (backedStorage.TryGetValue(key, out var partition))
                if (partition.TryGetValue(index, out var value))
                    return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<byte[]>>(Option.None);
        }

        public IAsyncEnumerable<StoreKey> GetKeys() => backedStorage.Keys.AsAsync();

        public void Dispose() { }
    }
}