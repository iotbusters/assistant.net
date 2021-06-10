using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Internal
{
    internal sealed class LocalStorageProvider<TValue> : IStorageProvider<TValue>
    {
        private readonly ConcurrentDictionary<StoreKey, byte[]> backedStorage = new();

        public async Task<byte[]> AddOrGet(StoreKey key, Func<StoreKey, Task<byte[]>> addFactory) =>
            backedStorage.GetOrAdd(key, await addFactory(key));

        public async Task<byte[]> AddOrUpdate(
            StoreKey key,
            Func<StoreKey, Task<byte[]>> addFactory,
            Func<StoreKey, byte[], Task<byte[]>> updateFactory)
        {
            var addValue = await addFactory(key);
            return backedStorage.AddOrUpdate(
                key,
                key => addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult(),
                (key, old) => updateFactory(key, old).ConfigureAwait(false).GetAwaiter().GetResult());
        }

        public Task<Option<byte[]>> TryGet(StoreKey key) =>
            Task.FromResult(backedStorage.TryGetValue(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public Task<Option<byte[]>> TryRemove(StoreKey key) =>
            Task.FromResult(backedStorage.TryRemove(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public IAsyncEnumerable<StoreKey> GetKeys() => backedStorage.Keys.AsAsync();

        void IDisposable.Dispose() { }
    }
}