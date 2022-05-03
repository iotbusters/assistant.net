using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
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
        private readonly IDiagnosticContext diagnosticContext;
        private readonly ISystemClock clock;

        /// <exception cref="ArgumentException"/>
        public PartitionedStorage(
            IServiceProvider provider,
            ITypeEncoder typeEncoder,
            IDiagnosticContext diagnosticContext,
            ISystemClock clock)
        {
            this.keyType = typeEncoder.Encode(typeof(TKey)) ?? throw NotSupportedTypeException(typeof(TKey));
            this.valueType = typeEncoder.Encode(typeof(TValue)) ?? throw NotSupportedTypeException(typeof(TValue));
            this.backedStorage = provider.GetService<IPartitionedStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException();
            this.keyConverter = provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException();
            this.valueConverter = provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException();
            this.diagnosticContext = diagnosticContext;
            this.clock = clock;
        }

        public async Task<long> Add(TKey key, TValue value, CancellationToken token)
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var content = await valueConverter.Convert(value, token);
            var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, 1);
            var valueRecord = new ValueRecord(valueType, content, audit);
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
            new($"Partitioned storage of '{typeof(TValue).Name}' wasn't properly configured.");

        private static NotSupportedException NotSupportedTypeException(Type type) =>
            new($"Type '{type.Name}' isn't supported.");
    }
}
