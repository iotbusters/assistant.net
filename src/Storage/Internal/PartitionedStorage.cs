using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class PartitionedStorage<TKey, TValue> : IPartitionedAdminStorage<TKey, TValue>
    {
        private readonly string keyType;
        private readonly string valueType;
        private readonly IValueConverter<TKey> keyConverter;
        private readonly IValueConverter<TValue> valueConverter;
        private readonly IPartitionedStorageProvider<TValue> backedStorage;

        /// <exception cref="ArgumentException"/>
        public PartitionedStorage(
            IServiceProvider provider,
            ITypeEncoder typeEncoder)
        {
            this.backedStorage = provider.GetService<IPartitionedStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyConverter = provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException();
            this.valueConverter = provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyType = typeEncoder.Encode(typeof(TKey));
            this.valueType = typeEncoder.Encode(typeof(TValue));
        }

        public async Task<long> Add(TKey key, TValue value, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var valueRecord = new ValueRecord(Type: valueType, Content: await valueConverter.Convert(value, token));
            return await backedStorage.Add(keyRecord, valueRecord, token);
        }

        public async Task<Option<TValue>> TryGet(TKey key, long index, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var option = await backedStorage.TryGet(keyRecord, index, token);
            return await option.MapOption(x => valueConverter.Convert(x.Content, token));
        }

        public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
            backedStorage.GetKeys()
                .Where(x => x.Type == keyType)
                .AsAsync()
                .Select(x => keyConverter.Convert(x.Content, token));

        private static ArgumentException ImproperlyConfiguredException() =>
            new($"Partition storage of '{typeof(TValue).Name}' wasn't properly configured.");
    }
}
