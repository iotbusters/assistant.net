using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class Storage<TKey, TValue> : IStorage<TKey, TValue>
    {
        private readonly IStorageProvider<TValue> backedStorage;
        private readonly IValueConverter<TValue> valueConverter;
        private readonly IKeyConverter<TKey> keyConverter;

        public Storage(
            IServiceProvider provider,
            IKeyConverter<TKey> keyConverter)
        {
            this.backedStorage = provider.GetService<IStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException;
            this.valueConverter = provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException;
            this.keyConverter = keyConverter;
        }

        private static InvalidOperationException ImproperlyConfiguredException =>
            new($"Storage of '{typeof(TValue).Name}' wasn't properly configured.");

        public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory)
        {
            var storeKey = await keyConverter.Convert(key);
            var value = await backedStorage.AddOrGet(storeKey, async _ => await valueConverter.Convert(await addFactory(key)));
            return await valueConverter.Convert(value);
        }

        public async Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory)
        {
            var storeKey = await keyConverter.Convert(key);
            var value = await backedStorage.AddOrUpdate(
                    storeKey,
                    async _ => await valueConverter.Convert(await addFactory(key)),
                    async (_, old) =>
                    {
                        var oldValue = await valueConverter.Convert(old);
                        var newValue = await updateFactory(key, oldValue);
                        return await valueConverter.Convert(newValue);
                    });
            return await valueConverter.Convert(value);
        }

        public async Task<Option<TValue>> TryGet(TKey key)
        {
            var storeKey = await keyConverter.Convert(key);
            var value = await backedStorage.TryGet(storeKey);
            return await value.Map(valueConverter.Convert);
        }

        public async Task<Option<TValue>> TryRemove(TKey key)
        {
            var storeKey = await keyConverter.Convert(key);
            var value = await backedStorage.TryRemove(storeKey);
            return await value.Map(valueConverter.Convert);
        }

        public IAsyncEnumerable<TKey> GetKeys() => 
            backedStorage.GetKeys()
                .Where(x => x.Type == keyConverter.KeyType)
                .Select(keyConverter.Convert);
    }
}