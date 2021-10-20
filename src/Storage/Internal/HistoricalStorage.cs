using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class HistoricalStorage<TKey, TValue> : Storage<TKey, TValue>, IHistoricalAdminStorage<TKey, TValue>
    {
        private readonly IHistoricalStorageProvider<TValue> backedStorage;

        /// <exception cref="ArgumentException"/>
        public HistoricalStorage(
            IServiceProvider provider,
            ITypeEncoder typeEncoder,
            IDiagnosticContext diagnosticContext)
            : base(
                provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException(typeof(TKey)),
                provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
                provider.GetService<IHistoricalStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
                typeEncoder,
                diagnosticContext) =>
            backedStorage = provider.GetService<IHistoricalStorageProvider<TValue>>()!;

        public async Task<Option<TValue>> TryGet(TKey key, long version, CancellationToken token)
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, version, token).MapOption(x => ValueConverter.Convert(x.Content, token));
        }

        public async Task<long> TryRemove(TKey key, long upToVersion, CancellationToken token)
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            return await backedStorage.TryRemove(keyRecord, upToVersion, token);
        }
    }
}