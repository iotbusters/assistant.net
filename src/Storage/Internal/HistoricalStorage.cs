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

internal class HistoricalStorage<TKey, TValue> : IHistoricalAdminStorage<TKey, TValue>
{
    private readonly IDiagnosticContext diagnosticContext;
    private readonly ISystemClock clock;
    private readonly string keyType;
    private readonly string valueType;
    private readonly IValueConverter<TKey> keyConverter;
    private readonly IValueConverter<TValue> valueConverter;
    private readonly IHistoricalStorageProvider<TValue> backedStorage;

    /// <exception cref="ArgumentException"/>
    public HistoricalStorage(
        IServiceProvider provider,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock,
        ITypeEncoder typeEncoder)
    {
        this.diagnosticContext = diagnosticContext;
        this.clock = clock;
        this.keyType = GetTypeName<TKey>(typeEncoder);
        this.valueType = GetTypeName<TValue>(typeEncoder);
        this.keyConverter = GetConverter<TKey>(provider);
        this.valueConverter = GetConverter<TValue>(provider);
        this.backedStorage = GetProvider(provider);
    }

    public async Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var valueRecord = await backedStorage.AddOrGet(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await valueConverter.Convert(value, token);
                    var audit = new Audit(1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                }, token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<HistoricalValue<TValue>> AddOrGet(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var valueRecord = await backedStorage.AddOrGet(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await valueConverter.Convert(value.Value, token);
                    var audit = new Audit(value.Details, 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                }, token);
            var value = await valueConverter.Convert(valueRecord.Content, token);
            return new HistoricalValue<TValue>(value, valueRecord.Audit.Details, valueRecord.Audit.Version);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<TValue> AddOrUpdate(TKey key, Func<TKey, Task<TValue>> addFactory, Func<TKey, TValue, Task<TValue>> updateFactory, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var valueRecord = await backedStorage.AddOrUpdate(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await valueConverter.Convert(value, token);
                    var audit = new Audit(1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                },
                updateFactory: async (_, old) =>
                {
                    var oldValue = await valueConverter.Convert(old.Content, token);
                    var newValue = await updateFactory(key, oldValue);
                    var content = await valueConverter.Convert(newValue, token);
                    var audit = new Audit(old.Audit.Version + 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                },
                token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<HistoricalValue<TValue>> AddOrUpdate(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, Func<TKey, StorageValue<TValue>, Task<StorageValue<TValue>>> updateFactory, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var valueRecord = await backedStorage.AddOrUpdate(
                keyRecord,
                addFactory: async _ =>
                {
                    var value = await addFactory(key);
                    var content = await valueConverter.Convert(value.Value, token);
                    var audit = new Audit(value.Details, 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                },
                updateFactory: async (_, old) =>
                {
                    var oldValue = new StorageValue<TValue>(
                        await valueConverter.Convert(old.Content, token),
                        old.Audit.Details);
                    var newValue = await updateFactory(key, oldValue);
                    var content = await valueConverter.Convert(newValue.Value, token);
                    var audit = new Audit(newValue.Details, old.Audit.Version + 1)
                    {
                        CorrelationId = diagnosticContext.CorrelationId,
                        User = diagnosticContext.User,
                        Created = clock.UtcNow
                    };
                    return new ValueRecord(valueType, content, audit);
                },
                token);
            var value = await valueConverter.Convert(valueRecord.Content, token);
            return new HistoricalValue<TValue>(value, valueRecord.Audit.Details, valueRecord.Audit.Version);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryGet(TKey key, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, token);
            return await option.MapOptionAsync(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<TValue>> TryGet(TKey key, long version, CancellationToken token)
    {
        if (version <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {version}.", nameof(version));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, version, token);
            return await option.MapOptionAsync(x => valueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<HistoricalValue<TValue>>> TryGetDetailed(TKey key, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, token);
            return await option.MapOptionAsync(async x =>
            {
                var value = await valueConverter.Convert(x.Content, token);
                return new HistoricalValue<TValue>(value, x.Audit.Details, x.Audit.Version);
            });
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<Option<HistoricalValue<TValue>>> TryGetDetailed(TKey key, long version, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, version, token);
            return await option.MapOptionAsync(async x =>
            {
                var value = await valueConverter.Convert(x.Content, token);
                return new HistoricalValue<TValue>(value, x.Audit.Details, x.Audit.Version);
            });
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public IAsyncEnumerable<TKey> GetKeys(CancellationToken token = default) =>
        backedStorage.GetKeys()
            .Where(x => x.Type == keyType && x.ValueType == valueType)
            .AsAsync()
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

    public async Task<long> TryRemove(TKey key, long upToVersion, CancellationToken token)
    {
        if (upToVersion <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.", nameof(upToVersion));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryRemove(keyRecord, upToVersion, token);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    private async Task<KeyRecord> CreateKeyRecord(TKey key, CancellationToken token)
    {
        var keyContent = await keyConverter.Convert(key, token);
        var keyId = keyContent.GetSha1();
        var keyRecord = new KeyRecord(keyId, keyType, keyContent, valueType);
        return keyRecord;
    }

    private static string GetTypeName<T>(ITypeEncoder typeEncoder) =>
        typeEncoder.Encode(typeof(T)) ?? throw new ArgumentException($"Type({typeof(T).Name}) isn't supported.");

    private static IHistoricalStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        return options.HistoricalProviders.TryGetValue(typeof(TValue), out var factory)
            ? (IHistoricalStorageProvider<TValue>)factory.Create(provider)
            : throw new ArgumentException($"HistoricalStorage({typeof(TValue).Name}) wasn't properly configured.");
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
