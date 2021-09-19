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
    internal class Storage<TKey, TValue> : IAdminStorage<TKey, TValue>
    {
        private readonly string keyType;
        private readonly string valueType;
        private readonly IValueConverter<TKey> keyConverter;
        private readonly IValueConverter<TValue> valueConverter;
        private readonly IStorageProvider<TValue> backedStorage;

        /// <exception cref="ArgumentException"/>
        public Storage(
            IServiceProvider provider,
            ITypeEncoder typeEncoder)
        {
            this.backedStorage = provider.GetService<IStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyConverter = provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException();
            this.valueConverter = provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyType = typeEncoder.Encode(typeof(TKey));
            this.valueType = typeEncoder.Encode(typeof(TValue));
        }

        public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var valueRecord = await backedStorage.AddOrGet(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    return new ValueRecord(Type: valueType, Content: await valueConverter.Convert(value, token));
                }, token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }

        public async Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory,
            CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var valueRecord = await backedStorage.AddOrUpdate(
                    keyRecord,
                    addFactory: async _ =>
                    {
                        var value = await addFactory(key);
                        return new ValueRecord(Type: valueType, Content: await valueConverter.Convert(value, token));
                    },
                    updateFactory: async (_, old) =>
                    {
                        var oldValue = await valueConverter.Convert(old.Content, token);
                        var newValue = await updateFactory(key, oldValue);
                        return new ValueRecord(Type: valueType, Content: await valueConverter.Convert(newValue, token));
                    },
                    token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }

        public async Task<Option<TValue>> TryGet(TKey key, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }

        public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryRemove(keyRecord, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }

        public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
            backedStorage.GetKeys()
                .Where(x => x.Type == keyType)
                .AsAsync()
                .Select(x => keyConverter.Convert(x.Content, token));

        private static ArgumentException ImproperlyConfiguredException() =>
            new($"Storage of '{typeof(TValue).Name}' wasn't properly configured.");
    }
}
