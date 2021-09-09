using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
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
        private readonly ISystemClock clock;
        private readonly IValueConverter<TKey> keyConverter;
        private readonly IValueConverter<TValue> valueConverter;
        private readonly IStorageProvider<TValue> backedStorage;

        /// <exception cref="ArgumentException"/>
        public Storage(
            IServiceProvider provider,
            ISystemClock clock,
            ITypeEncoder typeEncoder)
        {
            this.backedStorage = provider.GetService<IStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyConverter = provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException();
            this.valueConverter = provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException();
            this.clock = clock;
            this.keyType = typeEncoder.Encode(typeof(TKey));
        }

        public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                content: keyContent,
                type: keyType);
            var valueRecord = await backedStorage.AddOrGet(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    return new ValueRecord(
                        content: await valueConverter.Convert(value, token),
                        version: 1,
                        created: clock.UtcNow,
                        updated: null);
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
                content: keyContent,
                type: keyType);
            var valueRecord = await backedStorage.AddOrUpdate(
                    keyRecord,
                    addFactory: async _ =>
                    {
                        var value = await addFactory(key);
                        return new ValueRecord(
                            content: await valueConverter.Convert(value, token),
                            version: 1,
                            created: clock.UtcNow,
                            updated: null);
                    },
                    updateFactory: async (_, old) =>
                    {
                        var oldValue = await valueConverter.Convert(old.Content, token);
                        var newValue = await updateFactory(key, oldValue);
                        return new ValueRecord(
                            content: await valueConverter.Convert(newValue, token),
                            version: old.Version + 1,
                            created: old.Created,
                            updated: clock.UtcNow);
                    },
                    token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }

        public async Task<Option<TValue>> TryGet(TKey key, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                content: keyContent,
                type: keyType);
            return await backedStorage.TryGet(keyRecord, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }

        public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                content: keyContent,
                type: keyType);
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