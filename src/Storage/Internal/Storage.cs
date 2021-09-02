using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var value = await backedStorage.AddOrGet(storeKey, async _ => await valueConverter.Convert(await addFactory(key)), token);
            return await valueConverter.Convert(value, token);
        }

        public async Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory,
            CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var value = await backedStorage.AddOrUpdate(
                    storeKey,
                    async _ => await valueConverter.Convert(await addFactory(key), token),
                    async (_, old) =>
                    {
                        var oldValue = await valueConverter.Convert(old, token);
                        var newValue = await updateFactory(key, oldValue);
                        return await valueConverter.Convert(newValue, token);
                    },
                    token);
            return await valueConverter.Convert(value, token);
        }

        public async Task<Option<TValue>> TryGet(TKey key, CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var value = await backedStorage.TryGet(storeKey, token);
            return await value.Map(k => valueConverter.Convert(k, token));
        }

        public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var value = await backedStorage.TryRemove(storeKey, token);
            return await value.Map(k => valueConverter.Convert(k, token));
        }

        public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) => 
            backedStorage.GetKeys(token)
                .Where(x => x.Type == keyConverter.KeyType)
                .Select(key => keyConverter.Convert(key, token));
    }
}