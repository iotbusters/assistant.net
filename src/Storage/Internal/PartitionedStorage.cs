using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<long> Add(TKey key, TValue value, CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var storeValue = await valueConverter.Convert(value, token);
            return await backedStorage.Add(storeKey, storeValue, token);
        }

        public async Task<Option<TValue>> TryGet(TKey key, long index, CancellationToken token)
        {
            var storeKey = await keyConverter.Convert(key, token);
            var value = await backedStorage.TryGet(storeKey, index, token);
            return await value.Map(k => valueConverter.Convert(k, token));
        }

        public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
            backedStorage.GetKeys(token).Select(key => keyConverter.Convert(key, token));
    }
}