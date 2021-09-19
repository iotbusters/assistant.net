using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal sealed class LocalStorageProvider<TValue> : IStorageProvider<TValue>
    {
        private readonly ConcurrentDictionary<KeyRecord, ValueRecord> backedStorage = new();

        public async Task<ValueRecord> AddOrGet(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token) =>
            backedStorage.GetOrAdd(key, await addFactory(key));

        public Task<ValueRecord> AddOrUpdate(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken _)
        {
            return Task.FromResult(
                backedStorage.AddOrUpdate(
                    key,
                    k => addFactory(k).ConfigureAwait(false).GetAwaiter().GetResult(),
                    (k, old) => updateFactory(k, old).ConfigureAwait(false).GetAwaiter().GetResult()));
        }

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken _) =>
            Task.FromResult(backedStorage.TryGetValue(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken _) =>
            Task.FromResult(backedStorage.TryRemove(key, out var value)
                ? Option.Some(value)
                : Option.None);

        public IQueryable<KeyRecord> GetKeys() => backedStorage.Keys.AsQueryable();

        void IDisposable.Dispose() { }
    }
}
