using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Tests.Mocks
{
    public class TestStorage<T> : IStorageProvider<T>
    {
        private readonly Dictionary<StoreKey, byte[]> stored = new();

        public async Task<byte[]> AddOrGet(
            StoreKey key,
            Func<StoreKey, Task<byte[]>> addFactory,
            CancellationToken token)
        {
            stored.TryAdd(key, await addFactory(key));
            return stored[key];
        }

        public async Task<byte[]> AddOrUpdate(
            StoreKey key,
            Func<StoreKey, Task<byte[]>> addFactory,
            Func<StoreKey, byte[], Task<byte[]>> updateFactory,
            CancellationToken token)
        {
            if (stored.TryAdd(key, await addFactory(key)))
                stored[key] = await addFactory(key);
            return stored[key];
        }

        public Task<Option<byte[]>> TryGet(StoreKey key, CancellationToken token)
        {
            if(stored.TryGetValue(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<byte[]>>(Option.None);
        }

        public Task<Option<byte[]>> TryRemove(StoreKey key, CancellationToken token)
        {
            if(stored.Remove(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<byte[]>>(Option.None);
        }

        public IAsyncEnumerable<StoreKey> GetKeys(CancellationToken token) => stored.Keys.AsAsync();

        public void Dispose() { }

    }
}