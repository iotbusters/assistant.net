using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

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
            var storeKey = keyConverter.Convert(key);
            var storeValue = valueConverter.Convert(value);
            return await backedStorage.Add(storeKey, storeValue);
        }

        public async Task<Option<TValue>> TryGet(TKey key, long index)
        {
            var storeKey = keyConverter.Convert(key);
            var value = await backedStorage.TryGet(storeKey, index);
            return value.Map(valueConverter.Convert);
        }

        public IAsyncEnumerable<TKey> GetKeys() => backedStorage.GetKeys().Select(keyConverter.Convert);
    }
}