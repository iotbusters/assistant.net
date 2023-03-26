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
            var valueRecord = await backedStorage.Add(
                keyRecord,
                addFactory: _ =>
                {
                    var audit = new Audit(1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return Task.FromResult(new ValueRecord(content, audit));
                },
                updateFactory: (_, old) =>
                {
                    var audit = new Audit(old.Audit.Version + 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return Task.FromResult(new ValueRecord(content, audit));
                },
                token);
            return valueRecord.Audit.Version;
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<PartitionValue<TValue>> Add(TKey key, StorageValue<TValue> value, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var content = await valueConverter.Convert(value.Value, token);
            var valueRecord = await backedStorage.Add(
                keyRecord,
                addFactory: _ =>
                {
                    var audit = new Audit(value.Details, 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return Task.FromResult(new ValueRecord(content, audit));
                },
                updateFactory: (_, old) =>
                {
                    var audit = new Audit(value.Details, old.Audit.Version + 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return Task.FromResult(new ValueRecord(content, audit));
                },
                token);
            return new PartitionValue<TValue>(value.Value, valueRecord.Audit.Details, valueRecord.Audit.Version);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryGet(TKey key, long index, CancellationToken token)
    {
        if (index <= 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"Value must be bigger than 0 but it was {index}.");
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, index, token);
            return await option.MapOptionAsync(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<PartitionValue<TValue>>> TryGetDetailed(TKey key, long index, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, index, token);
            return await option.MapOptionAsync(async x =>
            {
                var value = await valueConverter.Convert(x.Content, token);
                return new PartitionValue<TValue>(value, x.Audit.Details, x.Audit.Version);
            });
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token) =>
        backedStorage.GetKeys(x => x.Type == keyType && x.ValueType == valueType, token)
            .Select(x => keyConverter.Convert(x.Content, token));

    public async Task<Option<TValue>> TryRemove(TKey key, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryRemove(keyRecord, token);
            return await option.MapOptionAsync(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryRemove(TKey key, long upToIndex, CancellationToken token)
    {
        if (upToIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(upToIndex), $"Value must be bigger than 0 but it was {upToIndex}.");
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryRemove(keyRecord, upToIndex, token);
            return await option.MapOptionAsync(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    protected async Task<KeyRecord> CreateKeyRecord(TKey key, CancellationToken token)
    {
        var keyContent = await keyConverter.Convert(key, token);
        var keyId = keyContent.GetSha1();
        var keyRecord = new KeyRecord(keyId, keyType, valueType, keyContent);
        return keyRecord;
    }

    private static string GetTypeName<T>(ITypeEncoder typeEncoder) =>
        typeEncoder.Encode(typeof(T)) ?? throw new ArgumentException($"Type({typeof(T).Name}) isn't supported.");

    private static IPartitionedStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        if(!options.PartitionedProviders.TryGetValue(typeof(TValue), out var factory)
           && options.AnyPartitionedProvider == null)
            throw new ArgumentException($"PartitionedStorage({typeof(TValue).Name}) wasn't properly configured.");

        var storageProvider = factory?.Create(provider) ?? options.AnyPartitionedProvider!.Create(provider, typeof(TValue));
        return (IPartitionedStorageProvider<TValue>)storageProvider;
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
