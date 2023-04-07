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

internal class Storage<TKey, TValue> : IAdminStorage<TKey, TValue>
{
    private readonly IDiagnosticContext diagnosticContext;
    private readonly ISystemClock clock;
    private readonly string keyType;
    private readonly string valueType;
    private readonly IStorageProvider<TValue> backedStorage;
    private readonly IValueConverter<TKey> keyConverter;
    private readonly IValueConverter<TValue> valueConverter;

    /// <exception cref="ArgumentException"/>
    public Storage(
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
                    return new(content, audit);
                }, token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<StorageValue<TValue>> AddOrGet(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, CancellationToken token)
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
                    return new(content, audit);
                }, token);
            var value = await valueConverter.Convert(valueRecord.Content, token);
            return new(value, valueRecord.Audit.Details);
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
                    return new(content, audit);
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
                    return new(content, audit);
                },
                token);
            return await valueConverter.Convert(valueRecord.Content, token);
        }
        catch (Exception ex) when (ex is not StorageException and not OperationCanceledException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<StorageValue<TValue>> AddOrUpdate(TKey key, Func<TKey, Task<StorageValue<TValue>>> addFactory, Func<TKey, StorageValue<TValue>, Task<StorageValue<TValue>>> updateFactory, CancellationToken token)
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
                    return new(content, audit);
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
                    return new(content, audit);
                },
                token);
            var value = await valueConverter.Convert(valueRecord.Content, token);
            return new(value, valueRecord.Audit.Details);
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

    public async Task<Option<StorageValue<TValue>>> TryGetDetailed(TKey key, CancellationToken token)
    {
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            var option = await backedStorage.TryGet(keyRecord, token);
            return await option.MapOptionAsync(async x =>
            {
                var value = await valueConverter.Convert(x.Content, token);
                return new StorageValue<TValue>(value, x.Audit.Details);
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

    private async Task<KeyRecord> CreateKeyRecord(TKey key, CancellationToken token)
    {
        var keyContent = await keyConverter.Convert(key, token);
        var keyId = keyContent.GetSha1();
        var keyRecord = new KeyRecord(keyId, keyType, valueType, keyContent);
        return keyRecord;
    }

    private static string GetTypeName<T>(ITypeEncoder typeEncoder) =>
        typeEncoder.Encode(typeof(T)) ?? throw new StorageException($"Type({typeof(T).Name}) isn't supported.");

    private static IStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;

        if (options.StorageProviderFactory == null)
            throw new StorageProviderNotRegisteredException();

        if (!options.IsAnyTypeAllowed && !options.Registrations.Contains(typeof(TValue)))
            throw new StoringTypeNotRegisteredException(typeof(TValue));

        return (IStorageProvider<TValue>)options.StorageProviderFactory.Create(provider, typeof(TValue));
    }

    private static IValueConverter<T> GetConverter<T>(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;

        if (options.Converters.TryGetValue(typeof(T), out var factory))
            return (IValueConverter<T>)factory.Create(provider);

        return provider.GetService<IValueConverter<T>>() ?? throw new ConverterNotRegisteredException(typeof(T));
    }
}
