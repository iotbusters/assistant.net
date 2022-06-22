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

internal class PartitionedStorage<TKey, TValue> : IPartitionedAdminStorage<TKey, TValue>
{
    private readonly string keyType;
    private readonly string valueType;
    private readonly IDiagnosticContext diagnosticContext;
    private readonly ISystemClock clock;
    private readonly IPartitionedStorageProvider<TValue> backedStorage;
    private readonly IValueConverter<TKey> keyConverter;
    private readonly IValueConverter<TValue> valueConverter;

    /// <exception cref="ArgumentException"/>
    public PartitionedStorage(
        IServiceProvider provider,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock,
        ITypeEncoder typeEncoder)
    {
        this.diagnosticContext = diagnosticContext;
        this.clock = clock;
        this.keyType = GetTypeName<TKey>(typeEncoder);
        this.valueType = GetTypeName<TValue>(typeEncoder);
        this.backedStorage = GetProvider(provider);
        this.keyConverter = GetConverter<TKey>(provider);
        this.valueConverter = GetConverter<TValue>(provider);
    }

    public async Task<long> Add(TKey key, TValue value, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
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
            var keyRecord = await CreateKeyRecord(key, token);
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

    public async Task<Option<Audit>> TryGetAudit(TKey key, long index, CancellationToken token)
    {
        if (index <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {index}.", nameof(index));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryGet(keyRecord, index, token).MapOption(x => x.Audit);
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
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryRemove(keyRecord, token).MapOption(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<long> TryRemove(TKey key, long upToIndex, CancellationToken token)
    {
        if (upToIndex <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToIndex}.", nameof(upToIndex));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryRemove(keyRecord, upToIndex, token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    protected async Task<KeyRecord> CreateKeyRecord(TKey key, CancellationToken token)
    {
        var keyContent = await keyConverter.Convert(key, token);
        var keyId = keyContent.GetSha1();
        var keyRecord = new KeyRecord(keyId, keyType, keyContent, valueType);
        return keyRecord;
    }

    private static string GetTypeName<T>(ITypeEncoder typeEncoder) =>
        typeEncoder.Encode(typeof(T)) ?? throw new ArgumentException($"Type({typeof(T).Name}) isn't supported.");

    private static IPartitionedStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        return options.PartitionedProviders.TryGetValue(typeof(TValue), out var factory)
            ? (IPartitionedStorageProvider<TValue>)factory.Create(provider)
            : throw new ArgumentException($"PartitionedStorage({typeof(TValue).Name}) wasn't properly configured.");
    }

    private static IValueConverter<T> GetConverter<T>(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;

        if (options.DefaultConverters.TryGetValue(typeof(T), out var defaultFactory))
            return (IValueConverter<T>)defaultFactory.Create(provider);

        return provider.GetService<IValueConverter<T>>()
               ?? throw new ArgumentException($"ValueConverter({typeof(T).Name}) wasn't properly configured.");
    }
}
