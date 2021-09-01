using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class PartitionedStorage<TKey, TValue> : IPartitionedStorage<TKey, TValue>
    {
        private readonly IPartitionedStorageProvider<TValue> backedStorage;
        private readonly IKeyConverter<TKey> keyConverter;
        private readonly IValueConverter<TValue> valueConverter;

        public PartitionedStorage(
            IServiceProvider provider,
            IKeyConverter<TKey> keyConverter,
            IValueConverter<TValue> valueConverter)
        {
            backedStorage = provider.GetService<IPartitionedStorageProvider<TValue>>()
                            ?? throw new InvalidOperationException($"Partition storage of '{typeof(TValue).Name}' wasn't properly configured.");
            this.keyConverter = keyConverter;
            this.valueConverter = valueConverter;
        }

        public async Task<long> Add(TKey key, TValue value)
        {
            var storeKey = await keyConverter.Convert(key);
            var storeValue = await valueConverter.Convert(value);
            return await backedStorage.Add(storeKey, storeValue);
        }

        public async Task<Option<TValue>> TryGet(TKey key, long index)
        {
            var storeKey = await keyConverter.Convert(key);
            var value = await backedStorage.TryGet(storeKey, index);
            return await value.Map(valueConverter.Convert);
        }

        public IAsyncEnumerable<TKey> GetKeys() => backedStorage.GetKeys().Select(keyConverter.Convert);
    }
}