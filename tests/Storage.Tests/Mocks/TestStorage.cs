using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Tests.Mocks
{
    public class TestStorage : IStorage<object>
    {
        private readonly Dictionary<string, object> stored = new();

        public IDictionary<string, object> Stored => stored;

        public async Task<object> AddOrGet(string key, Func<string, Task<object>> addFactory)
        {
            stored.TryAdd(key, await addFactory(key));
            return stored[key];
        }

        public async Task<object> AddOrUpdate(string key, Func<string, Task<object>> addFactory, Func<string, object, Task<object>> updateFactory)
        {
            if (stored.TryAdd(key, await addFactory(key)))
                stored[key] = await addFactory(key);
            return stored[key];
        }

        public Task<Option<object>> TryGet(string key)
        {
            if(stored.TryGetValue(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<object>>(Option.None);
        }

        public Task<Option<object>> TryRemove(string key)
        {
            if(stored.Remove(key, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<object>>(Option.None);
        }

        public void Dispose() { }
    }
}