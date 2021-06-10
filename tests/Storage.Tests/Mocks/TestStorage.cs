using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Tests.Mocks
{
    public class TestStorage<T> : IStorageProvider<T>
    {
        private readonly Dictionary<StoreKey, byte[]> stored = new();

        public IDictionary<StoreKey, byte[]> Stored => stored;

        public async Task<byte[]> AddOrGet(
            StoreKey key,
            Func<StoreKey, Task<byte[]>> addFactory)
        {
            stored.TryAdd(key, await addFactory(key));
            return stored[key];
        }

        public async Task<byte[]> AddOrUpdate(
            StoreKey key,
            Func<StoreKey, Task<byte[]>> addFactory,
            Func<StoreKey, byte[], Task<byte[]>> updateFactory)
        {
            if (stored.TryAdd(key, await addFactory(key)))
                stored[key] = await addFactory(key);
            return stored[key];
        }

        public Task<Option<byte[]>> TryGet(StoreKey key)
        {
            if(stored.TryGetValue(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<byte[]>>(Option.None);
        }

        public Task<Option<byte[]>> TryRemove(StoreKey key)
        {
            if(stored.Remove(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<byte[]>>(Option.None);
        }

        public IAsyncEnumerable<StoreKey> GetKeys() => stored.Keys.AsAsync();

        public void Dispose() { }

    }
}