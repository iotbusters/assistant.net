using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
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
    private readonly IDiagnosticContext diagnosticContext;
    private readonly ISystemClock clock;
    protected readonly string KeyType;
    protected readonly string ValueType;
    protected readonly IStorageProvider<TValue> BackedStorage;
    protected readonly IValueConverter<TKey> KeyConverter;
    protected readonly IValueConverter<TValue> ValueConverter;

    /// <exception cref="ArgumentException"/>
    protected Storage(
        IServiceProvider provider,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock,
        ITypeEncoder typeEncoder,
        IStorageProvider<TValue> backedStorage)
    {
        this.diagnosticContext = diagnosticContext;
        this.clock = clock;
        this.KeyType = GetTypeName<TKey>(typeEncoder);
        this.ValueType = GetTypeName<TValue>(typeEncoder);
        this.BackedStorage = backedStorage;
        this.KeyConverter = GetConverter<TKey>(provider);
        this.ValueConverter = GetConverter<TValue>(provider);
    }

    /// <exception cref="ArgumentException"/>
    public Storage(
        IServiceProvider provider,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock,
        ITypeEncoder typeEncoder) : this(
        provider,
        diagnosticContext,
        clock,
        typeEncoder,
        GetProvider(provider))
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
            var valueRecord = await BackedStorage.AddOrGet(
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
            var valueRecord = await BackedStorage.AddOrUpdate(
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
            return await BackedStorage.TryGet(keyRecord, token).MapOption(x => ValueConverter.Convert(x.Content, token));
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
            return await BackedStorage.TryGet(keyRecord, token).MapOption(x => x.Audit);
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
            return await BackedStorage.TryRemove(keyRecord, token).MapOption(x => ValueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
        BackedStorage.GetKeys()
            .Where(x => x.Type == KeyType)
            .AsAsync()
            .Select(x => KeyConverter.Convert(x.Content, token));

    private static string GetTypeName<T>(ITypeEncoder typeEncoder) =>
        typeEncoder.Encode(typeof(T)) ?? throw new ArgumentException($"Type({typeof(T).Name}) isn't supported.");

    private static IStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        return options.Providers.TryGetValue(typeof(TValue), out var factory)
            ? (IStorageProvider<TValue>)factory.Create(provider)
            : throw new ArgumentException($"Storage({typeof(TValue).Name}) wasn't properly configured.");
    }

    private static IValueConverter<T> GetConverter<T>(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        return options.Converters.TryGetValue(typeof(T), out var factory)
            ? (IValueConverter<T>)factory.Create(provider)
            : provider.GetService<IValueConverter<T>>()
              ?? throw new ArgumentException($"ValueConverter({typeof(T).Name}) wasn't properly configured.");
    }
}
