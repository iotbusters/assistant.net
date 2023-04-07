using Assistant.Net.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage options extension for local providers.
/// </summary>
public static class StorageOptionsExtensions
{
    /// <summary>
    ///     Configures storage to use a storage provider implementation factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered storage provider.
    /// </remarks>
    public static StorageOptions UseStorage(this StorageOptions options, Func<IServiceProvider, Type, IStorageProvider> factory)
    {
        options.StorageProviderFactory = new InstanceCachingFactory<IStorageProvider, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use a historical storage provider implementation factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered historical storage provider.
    /// </remarks>
    public static StorageOptions UseHistoricalStorage(this StorageOptions options, Func<IServiceProvider, Type, IHistoricalStorageProvider> factory)
    {
        options.HistoricalStorageProviderFactory = new InstanceCachingFactory<IHistoricalStorageProvider, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use a partitioned storage provider implementation factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered partitioned storage provider.
    /// </remarks>
    public static StorageOptions UsePartitionedStorage(this StorageOptions options, Func<IServiceProvider, Type, IPartitionedStorageProvider> factory)
    {
        options.PartitionedStorageProviderFactory = new InstanceCachingFactory<IPartitionedStorageProvider, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use a local provider implementation factories.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered storage provider factories.
    /// </remarks>
    public static StorageOptions UseLocal(this StorageOptions options) => options
        .UseStorage((p, valueType) =>
        {
            var implementationType = typeof(LocalStorageProvider<>).MakeGenericType(valueType);
            return (IStorageProvider)p.GetRequiredService(implementationType);
        })
        .UseHistoricalStorage((p, valueType) =>
        {
            var implementationType = typeof(LocalHistoricalStorageProvider<>).MakeGenericType(valueType);
            return (IHistoricalStorageProvider)p.GetRequiredService(implementationType);
        })
        .UsePartitionedStorage((p, valueType) =>
        {
            var implementationType = typeof(LocalPartitionedStorageProvider<>).MakeGenericType(valueType);
            return (IPartitionedStorageProvider)p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Adds <typeparamref name="TValue"/> type storages.
    /// </summary>
    public static StorageOptions AddType<TValue>(this StorageOptions options) => options
        .AddType(typeof(TValue));

    /// <summary>
    ///     Adds <paramref name="storingType"/> storages.
    /// </summary>
    public static StorageOptions AddType(this StorageOptions options, Type storingType)
    {
        options.Registrations.Add(storingType);
        return options;
    }

    /// <summary>
    ///     Allows storing of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, <see cref="StorageOptions.Registrations"/> will be ignored.
    /// </remarks>
    public static StorageOptions AllowAnyType(this StorageOptions options)
    {
        options.IsAnyTypeAllowed = true;
        return options;
    }

    /// <summary>
    ///     Disallows storing of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, <see cref="StorageOptions.Registrations"/> should be configured. Default value: disallowed.
    /// </remarks>
    public static StorageOptions DisallowAnyType(this StorageOptions options)
    {
        options.IsAnyTypeAllowed = false;
        return options;
    }

    /// <summary>
    ///     Removes <typeparamref name="TValue"/> type storages.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method wont impart any type storage configurations.
    /// </remarks>
    public static StorageOptions RemoveType<TValue>(this StorageOptions options) => options
        .RemoveType(typeof(TValue));

    /// <summary>
    ///     Removes <paramref name="storingType"/> storages.
    /// </summary>
    public static StorageOptions RemoveType(this StorageOptions options, Type storingType)
    {
        options.Registrations.Remove(storingType);
        return options;
    }
}
