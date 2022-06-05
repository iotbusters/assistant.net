using Assistant.Net.Options;
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
    private static readonly Type localProviderType = typeof(LocalStorageProvider<>);
    private static readonly Type localHistoricalProviderType = typeof(LocalHistoricalStorageProvider<>);
    private static readonly Type localPartitionedProviderType = typeof(LocalPartitionedStorageProvider<>);

    /// <summary>
    ///     Configures storage to use a local single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseLocalSingleProvider(this StorageOptions options)
    {
        options.SingleProvider = new((p, valueType) =>
        {
            var implementationType = localProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        options.SingleHistoricalProvider = new((p, valueType) =>
        {
            var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        options.SinglePartitionedProvider = new((p, valueType) =>
        {
            var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSingle(this StorageOptions options, Type valueType)
    {
        options.Providers[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var factory = options.SingleProvider
                          ?? throw new ArgumentException("Single storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSingleAny(this StorageOptions options)
    {
        options.ProviderAny = new InstanceCachingFactory<object, Type>((p, valueType) =>
        {
            var factory = options.SingleProvider
                          ?? throw new ArgumentException("Single storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single historical storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSingleHistorical(this StorageOptions options, Type valueType)
    {
        options.HistoricalProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var factory = options.SingleHistoricalProvider
                          ?? throw new ArgumentException("Single historical storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single historical storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSingleHistoricalAny(this StorageOptions options)
    {
        options.HistoricalProviderAny = new InstanceCachingFactory<object, Type>((p, valueType) =>
        {
            var factory = options.SingleHistoricalProvider
                          ?? throw new ArgumentException("Single historical storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single partitioned storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSinglePartitioned(this StorageOptions options, Type valueType)
    {
        options.PartitionedProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var factory = options.SinglePartitionedProvider
                          ?? throw new ArgumentException("Single partitioned storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers single partitioned storage provider of an type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    ///     It requires calling one of Use***SingleProvider method.
    /// </remarks>
    public static StorageOptions AddSinglePartitionedAny(this StorageOptions options)
    {
        options.PartitionedProviderAny = new InstanceCachingFactory<object, Type>((p, valueType) =>
        {
            var factory = options.SinglePartitionedProvider
                          ?? throw new ArgumentException("Single partitioned storage provider wasn't properly configured.");
            return factory.Create(p, valueType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocal(this StorageOptions options, Type valueType)
    {
        options.Providers[valueType] = new(p =>
        {
            var implementationType = localProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocalAny(this StorageOptions options)
    {
        options.ProviderAny = new((p, valueType) =>
        {
            var implementationType = localProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local historical storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocalHistorical(this StorageOptions options, Type valueType)
    {
        options.HistoricalProviders[valueType] = new(p =>
        {
            var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local historical storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocalHistoricalAny(this StorageOptions options)
    {
        options.HistoricalProviderAny = new((p, valueType) =>
        {
            var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local partitioned storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocalPartitioned(this StorageOptions options, Type valueType)
    {
        options.PartitionedProviders[valueType] = new(p =>
        {
            var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers local partitioned storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddLocalPartitionedAny(this StorageOptions options)
    {
        options.PartitionedProviderAny = new((p, valueType) =>
        {
            var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }
}
