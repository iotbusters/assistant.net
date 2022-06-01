using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring local storages.
/// </summary>
public static class StorageBuilderExtensions
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
    public static StorageBuilder UseLocalSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .TryAddSingleton(localProviderType, localProviderType)
            .TryAddSingleton(localHistoricalProviderType, localHistoricalProviderType)
            .TryAddSingleton(localPartitionedProviderType, localPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
            {
                o.SingleProvider = new((p, valueType) =>
                {
                    var implementationType = localProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SingleHistoricalProvider = new((p, valueType) =>
                {
                    var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SinglePartitionedProvider = new((p, valueType) =>
                {
                    var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
            });
        return builder;
    }

    /// <summary>
    ///     Adds single provider based storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder Add<TKey, TValue>(this StorageBuilder builder) => builder
        .Add(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds single provider based storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder Add(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.Providers[valueType] = new(p =>
                {
                    var factory = o.SingleProvider
                                  ?? throw new ArgumentException("Single storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds single provider based storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.ProviderAny = new((p, valueType) =>
                {
                    var factory = o.SingleProvider
                                  ?? throw new ArgumentException("Single storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviders[valueType] = new(p =>
                {
                    var factory = o.SingleHistoricalProvider
                                  ?? throw new ArgumentException("Single historical storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviderAny = new((p, valueType) =>
                {
                    var factory = o.SingleHistoricalProvider
                                  ?? throw new ArgumentException("Single historical storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddPartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local partitioned storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered providers.
    /// </remarks>
    public static StorageBuilder AddPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviders[valueType] = new(p =>
                {
                    var factory = o.SinglePartitionedProvider
                                  ?? throw new ArgumentException("Single partitioned storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddPartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviderAny = new((p, valueType) =>
                {
                    var factory = o.SinglePartitionedProvider
                                  ?? throw new ArgumentException("Single partitioned storage provider wasn't properly configured.");
                    return factory.Create(p, valueType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocal<TKey, TValue>(this StorageBuilder builder) => builder
        .AddLocal(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocal(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .TryAddSingleton(localProviderType, localProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.Providers[valueType] = new(p =>
                {
                    var implementationType = localProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalAny(this StorageBuilder builder)
    {
        builder.Services
            .TryAddSingleton(localProviderType, localProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.ProviderAny = new((p, valueType) =>
                {
                    var implementationType = localProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddLocalHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .TryAddSingleton(localHistoricalProviderType, localHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviders[valueType] = new(p =>
                {
                    var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .TryAddSingleton(localHistoricalProviderType, localHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviderAny = new((p, valueType) =>
                {
                    var implementationType = localHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddLocalPartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local partitioned storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .TryAddSingleton(localPartitionedProviderType, localPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviders[valueType] = new(p =>
                {
                    var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddLocalPartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .TryAddSingleton(localPartitionedProviderType, localPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviderAny = new((p, valueType) =>
                {
                    var implementationType = localPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Apply a configuration type <typeparamref name="TConfiguration" />.
    /// </summary>
    public static StorageBuilder AddConfiguration<TConfiguration>(this StorageBuilder builder)
        where TConfiguration : IStorageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Apply a list of configuration instances <paramref name="storageConfigurations" />.
    /// </summary>
    public static StorageBuilder AddConfiguration(this StorageBuilder builder, params IStorageConfiguration[] storageConfigurations)
    {
        foreach (var config in storageConfigurations)
            config.Configure(builder);
        return builder;
    }
}
