using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Tests.Mocks
{
    public class TestStorage<T> : IStorageProvider<T>
    {
        private readonly Dictionary<KeyRecord, ValueRecord> stored = new();

        public async Task<ValueRecord> AddOrGet(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            CancellationToken token)
        {
            stored.TryAdd(key, await addFactory(key));
            return stored[key];
        }

        public async Task<ValueRecord> AddOrUpdate(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken token)
        {
            if (stored.TryAdd(key, await addFactory(key)))
                stored[key] = await addFactory(key);
            return stored[key];
        }

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
        {
            if(stored.TryGetValue(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<ValueRecord>>(Option.None);
        }

        public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
        {
            if(stored.Remove(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<ValueRecord>>(Option.None);
        }

        public IQueryable<KeyRecord> GetKeys() => stored.Keys.AsQueryable();

        public void Dispose() { }

    }
}