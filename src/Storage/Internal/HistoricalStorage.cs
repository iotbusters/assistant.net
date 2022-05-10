using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
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
            IDiagnosticContext diagnosticContext,
            ISystemClock clock)
            : base(
                provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException(typeof(TKey)),
                provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
                provider.GetService<IHistoricalStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
                typeEncoder,
                diagnosticContext,
                clock) =>
            backedStorage = provider.GetService<IHistoricalStorageProvider<TValue>>()!;

        public async Task<Option<TValue>> TryGet(TKey key, long version, CancellationToken token)
        {
            if (version <= 0)
                throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {version}.", nameof(version));
            try
            {
                var keyContent = await KeyConverter.Convert(key, token);
                var keyRecord = new KeyRecord(
                    id: keyContent.GetSha1(),
                    type: KeyType,
                    content: keyContent);
                return await backedStorage.TryGet(keyRecord, version, token).MapOption(x => ValueConverter.Convert(x.Content, token));
            }
            catch (Exception ex)
            {
                throw new StorageException(ex);
            }
        }

        public async Task<long> TryRemove(TKey key, long upToVersion, CancellationToken token)
        {
            if (upToVersion <= 0)
                throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.", nameof(upToVersion));
            try
            {
                var keyContent = await KeyConverter.Convert(key, token);
                var keyRecord = new KeyRecord(
                    id: keyContent.GetSha1(),
                    type: KeyType,
                    content: keyContent);
                return await backedStorage.TryRemove(keyRecord, upToVersion, token);
            }
            catch (Exception ex)
            {
                throw new StorageException(ex);
            }
        }
    }
}
