using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

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
        try
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            var content = await valueConverter.Convert(value, token);
            return await backedStorage.Add(
                keyRecord,
                addFactory: _ =>
                {
                    var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, 1);
                    return Task.FromResult(new ValueRecord(valueType, content, audit));
                },
                updateFactory: (_, old) =>
                {
                    var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, old.Audit.Version + 1);
                    return Task.FromResult(new ValueRecord(valueType, content, audit));
                },
                token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryGet(TKey key, long index, CancellationToken token)
    {
        if (index <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {index}.", nameof(index));
        try
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, index, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
        backedStorage.GetKeys()
            .Where(x => x.Type == keyType)
            .AsAsync()
            .Select(x => keyConverter.Convert(x.Content, token));

    public async Task<Option<Audit>> TryGetAudit(TKey key, long index, CancellationToken token = default)
    {
        if (index <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {index}.", nameof(index));
        try
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, index, token).MapOption(x => x.Audit);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token = default)
    {
        try
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryRemove(keyRecord, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<long> TryRemove(TKey key, long upToIndex, CancellationToken token = default)
    {
        if (upToIndex <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToIndex}.", nameof(upToIndex));
        try
        {
            var keyContent = await keyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: keyType,
                content: keyContent);
            return await backedStorage.TryRemove(keyRecord, upToIndex, token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    private static ArgumentException ImproperlyConfiguredException() =>
        new($"Partitioned storage of '{typeof(TValue).Name}' wasn't properly configured.");

    private static NotSupportedException NotSupportedTypeException(Type type) =>
        new($"Type '{type.Name}' isn't supported.");
}
