using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Internal
{
    internal sealed class LocalStorage<TValue> : IStorage<TValue>
    {
        private readonly ConcurrentDictionary<string, Task<TValue>> backedStorage = new();

        public Task<TValue> AddOrGet(string key, Func<string, Task<TValue>> addFactory) =>
            backedStorage.GetOrAdd(key, addFactory);

        public Task<TValue> AddOrUpdate(
            string key,
            Func<string, Task<TValue>> addFactory,
            Func<string, TValue, Task<TValue>> updateFactory) =>
            backedStorage.AddOrUpdate(
                key,
                addFactory,
                async (key, old) => await updateFactory(key, await old));

        public async Task<Option<TValue>> TryGet(string key) =>
            backedStorage.TryGetValue(key, out var task)
                ? Option.Some(await task)
                : Option.None;

        public async Task<Option<TValue>> TryRemove(string key) =>
            backedStorage.TryRemove(key, out var task)
                ? Option.Some(await task)
                : Option.None;

        void IDisposable.Dispose()
        {
            if (typeof(TValue).IsAssignableTo(typeof(IDisposable)))
                foreach (var value in backedStorage.Values)
                    ((IDisposable)value).Dispose();
            GC.SuppressFinalize(this);
        }

        ~LocalStorage() => ((IDisposable)this).Dispose();
    }
}