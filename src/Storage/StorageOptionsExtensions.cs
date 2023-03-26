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
    ///     Configures storage to use single storage provider implementation.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">Single storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseSingleProvider(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.SingleProvider = new InstanceCachingFactory<object, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use single historical storage provider implementation.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">Single historical storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseSingleHistoricalProvider(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.SingleHistoricalProvider = new InstanceCachingFactory<object, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use single partitioned storage provider implementation.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">single partitioned storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseSinglePartitionedProvider(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.SinglePartitionedProvider = new InstanceCachingFactory<object, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Configures storage to use a local single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseLocalSingleProvider(this StorageOptions options) => options
        .UseSingleProvider((p, valueType) =>
        {
            var implementationType = localProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        })
        .UseSingleHistoricalProvider((p, valueType) =>
        {
            var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        })
        .UseSinglePartitionedProvider((p, valueType) =>
        {
            var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers storage provider of any type if none defined explicitly.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">Storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddAny(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.AnyProvider = new InstanceCachingFactory<object, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Registers historical storage provider of any type if none defined explicitly.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">Historical storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddHistoricalAny(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.AnyHistoricalProvider = new InstanceCachingFactory<object, Type>(factory);
        return options;
    }

    /// <summary>
    ///     Registers partitioned storage provider of any type if none defined explicitly.
    /// </summary>
    /// <param name="options"/>
    /// <param name="factory">Partitioned storage provider factory.</param>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddPartitionedAny(this StorageOptions options, Func<IServiceProvider, Type, object> factory)
    {
        options.AnyPartitionedProvider = new InstanceCachingFactory<object, Type>(factory);
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
        options.Providers[valueType] = new(p =>
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
        options.AnyProvider = new((p, valueType) =>
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
        options.HistoricalProviders[valueType] = new(p =>
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
        options.AnyHistoricalProvider = new((p, valueType) =>
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
        options.PartitionedProviders[valueType] = new(p =>
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
        options.AnyPartitionedProvider = new((p, valueType) =>
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
        options.Providers[valueType] = new InstanceCachingFactory<object>(p =>
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
        options.AnyProvider = new((p, valueType) =>
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
        options.HistoricalProviders[valueType] = new InstanceCachingFactory<object>(p =>
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
        options.AnyHistoricalProvider = new((p, valueType) =>
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
        options.PartitionedProviders[valueType] = new InstanceCachingFactory<object>(p =>
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
        options.AnyPartitionedProvider = new((p, valueType) =>
        {
            var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Removes a storage provider of <typeparamref name="TValue"/> type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method wont impart any type storage configurations.
    /// </remarks>
    public static StorageOptions Remove<TValue>(this StorageOptions options) => options
        .Remove(typeof(TValue));

    /// <summary>
    ///     Removes a storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method wont impart any type storage configurations.
    /// </remarks>
    public static StorageOptions Remove(this StorageOptions options, Type valueType)
    {
        options.Providers.Remove(valueType);
        options.HistoricalProviders.Remove(valueType);
        options.PartitionedProviders.Remove(valueType);
        return options;
    }
}
