using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal sealed class LocalStorageProvider<TValue> : IStorageProvider<TValue>
    {
        private readonly ConcurrentDictionary<StoreKey, byte[]> backedStorage = new();

        public async Task<byte[]> AddOrGet(StoreKey key, Func<StoreKey, Task<byte[]>> addFactory, CancellationToken token) =>
            backedStorage.GetOrAdd(key, await addFactory(key));

        public Task<byte[]> AddOrUpdate(
            StoreKey keyObj,
            Func<StoreKey, Task<byte[]>> addFactory,
            Func<StoreKey, byte[], Task<byte[]>> updateFactory,
            CancellationToken token)
        {
            return Task.FromResult(
                backedStorage.AddOrUpdate(
                    keyObj,
                    key => addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult(),
                    (key, old) => updateFactory(key, old).ConfigureAwait(false).GetAwaiter().GetResult()));
        }

        public Task<Option<byte[]>> TryGet(StoreKey key, CancellationToken token) =>
            Task.FromResult(backedStorage.TryGetValue(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public Task<Option<byte[]>> TryRemove(StoreKey key, CancellationToken token) =>
            Task.FromResult(backedStorage.TryRemove(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public IAsyncEnumerable<StoreKey> GetKeys(CancellationToken token) => backedStorage.Keys.AsAsync();

        void IDisposable.Dispose() { }
    }
}