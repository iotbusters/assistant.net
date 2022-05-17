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

internal class Storage<TKey, TValue> : IAdminStorage<TKey, TValue>
{
    protected readonly string KeyType;
    protected readonly string ValueType;
    protected readonly IValueConverter<TKey> KeyConverter;
    protected readonly IValueConverter<TValue> ValueConverter;
    private readonly IStorageProvider<TValue> backedStorage;
    private readonly IDiagnosticContext diagnosticContext;
    private readonly ISystemClock clock;

    /// <exception cref="NotSupportedException"/>
    protected Storage(
        IValueConverter<TKey> keyConverter,
        IValueConverter<TValue> valueConverter,
        IStorageProvider<TValue> backedStorage,
        ITypeEncoder typeEncoder,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock)
    {
        this.KeyType = typeEncoder.Encode(typeof(TKey)) ?? throw NotSupportedTypeException(typeof(TKey));
        this.ValueType = typeEncoder.Encode(typeof(TValue)) ?? throw NotSupportedTypeException(typeof(TValue));
        this.KeyConverter = keyConverter;
        this.ValueConverter = valueConverter;
        this.backedStorage = backedStorage;
        this.diagnosticContext = diagnosticContext;
        this.clock = clock;
    }

    /// <exception cref="ArgumentException"/>
    /// <exception cref="NotSupportedException"/>
    public Storage(
        IServiceProvider provider,
        ITypeEncoder typeEncoder,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock)
        : this(
            provider.GetService<IValueConverter<TKey>>() ?? throw ImproperlyConfiguredException(typeof(TKey)),
            provider.GetService<IValueConverter<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
            provider.GetService<IStorageProvider<TValue>>() ?? throw ImproperlyConfiguredException(typeof(TValue)),
            typeEncoder,
            diagnosticContext,
            clock)
    {
    }

    public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token)
    {
        try
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            var valueRecord = await backedStorage.AddOrGet(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await ValueConverter.Convert(value, token);
                    var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, 1);
                    return new ValueRecord(ValueType, content, audit);
                }, token);
            return await ValueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<TValue> AddOrUpdate(
        TKey key,
        Func<TKey, Task<TValue>> addFactory,
        Func<TKey, TValue, Task<TValue>> updateFactory,
        CancellationToken token)
    {
        try
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            var valueRecord = await backedStorage.AddOrUpdate(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await ValueConverter.Convert(value, token);
                    var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, 1);
                    return new ValueRecord(ValueType, content, audit);
                },
                updateFactory: async (_, old) =>
                {
                    var oldValue = await ValueConverter.Convert(old.Content, token);
                    var newValue = await updateFactory(key, oldValue);
                    var content = await ValueConverter.Convert(newValue, token);
                    var audit = new Audit(diagnosticContext.CorrelationId, diagnosticContext.User, clock.UtcNow, old.Audit.Version + 1);
                    return new ValueRecord(ValueType, content, audit);
                },
                token);
            return await ValueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryGet(TKey key, CancellationToken token)
    {
        try
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, token).MapOption(x => ValueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<Audit>> TryGetAudit(TKey key, CancellationToken token = default)
    {
        try
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            return await backedStorage.TryGet(keyRecord, token).MapOption(x => x.Audit);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token)
    {
        try
        {
            var keyContent = await KeyConverter.Convert(key, token);
            var keyRecord = new KeyRecord(
                id: keyContent.GetSha1(),
                type: KeyType,
                content: keyContent);
            return await backedStorage.TryRemove(keyRecord, token).MapOption(x => ValueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
        backedStorage.GetKeys()
            .Where(x => x.Type == KeyType)
            .AsAsync()
            .Select(x => KeyConverter.Convert(x.Content, token));

    protected static ArgumentException ImproperlyConfiguredException(Type type) =>
        new($"Storage of '{type.Name}' wasn't properly configured.");

    protected static NotSupportedException NotSupportedTypeException(Type type) =>
        new($"Type '{type.Name}' isn't supported.");
}
