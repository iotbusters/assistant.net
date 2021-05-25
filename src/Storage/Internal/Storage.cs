using System;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage.Internal
{
    internal class Storage<TKey, TValue> : IStorage<TKey, TValue>
    {
        private readonly IStorage<TValue> backedStorage;
        private readonly IKeyConverter<TKey> keyConverter;

        public Storage(IServiceProvider provider, IKeyConverter<TKey> keyConverter)
        {
            this.backedStorage = provider.GetService<IStorage<TValue>>()
                                 ?? throw new InvalidOperationException($"Storage of {typeof(TValue).Name} wasn't properly configured.");
            this.keyConverter = keyConverter;
        }

        public Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory)
        {
            var keyString = keyConverter.Convert(key);
            return backedStorage.AddOrGet(keyString, _ => addFactory(key));
        }

        public Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory)
        {
            var keyString = keyConverter.Convert(key);
            return backedStorage.AddOrUpdate(
                keyString,
                keyString => addFactory(key),
                (keyString, old) => updateFactory(key, old));
        }

        public Task<Option<TValue>> TryGet(TKey key)
        {
            var keyString = keyConverter.Convert(key);
            return backedStorage.TryGet(keyString);
        }

        public Task<Option<TValue>> TryRemove(TKey key)
        {
            var keyString = keyConverter.Convert(key);
            return backedStorage.TryRemove(keyString);
        }
    }
}