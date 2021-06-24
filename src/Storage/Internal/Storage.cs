using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

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
            new InvalidOperationException($"Storage of '{typeof(TValue).Name}' wasn't properly configured.");

        public Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory)
        {
            var storeKey = keyConverter.Convert(key);
            return backedStorage
                .AddOrGet(storeKey, _ => addFactory(key).MapSuccess(valueConverter.Convert))
                .MapSuccess(x => valueConverter.Convert(x));
        }

        public Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory)
        {
            var storeKey = keyConverter.Convert(key);
            return backedStorage
                .AddOrUpdate(
                    storeKey,
                    _ => addFactory(key).MapSuccess(valueConverter.Convert),
                    (_, old) => updateFactory(key, valueConverter.Convert(old)).MapSuccess(valueConverter.Convert))
                .MapSuccess(x => valueConverter.Convert(x));
        }

        public Task<Option<TValue>> TryGet(TKey key)
        {
            var storeKey = keyConverter.Convert(key);
            return backedStorage.TryGet(storeKey).Map(valueConverter.Convert);
        }

        public Task<Option<TValue>> TryRemove(TKey key)
        {
            var storeKey = keyConverter.Convert(key);
            return backedStorage.TryRemove(storeKey).MapSuccess(x => x.Map(valueConverter.Convert));
        }

        public IAsyncEnumerable<TKey> GetKeys() => 
            backedStorage.GetKeys()
                .Where(x => x.Type == keyConverter.KeyType)
                .Select(keyConverter.Convert);
    }
}