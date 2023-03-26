using Assistant.Net.Options;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage options extensions for MongoDB provider.
/// </summary>
public static class StorageOptionsExtensions
{
    private static readonly Type mongoProviderType = typeof(MongoStorageProvider<>);
    private static readonly Type mongoHistoricalProviderType = typeof(MongoHistoricalStorageProvider<>);
    private static readonly Type mongoPartitionedProviderType = typeof(MongoPartitionedStorageProvider<>);

    /// <summary>
    ///     Configures storage to use a MongoDB single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseMongoSingleProvider(this StorageOptions options) => options
        .UseSingleProvider((p, valueType) =>
        {
            var implementationType = mongoProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        })
        .UseSingleHistoricalProvider((p, valueType) =>
        {
            var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        })
        .UseSinglePartitionedProvider((p, valueType) =>
        {
            var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers MongoDB storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongo(this StorageOptions options, Type valueType)
    {
        options.Providers[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = mongoProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoAny(this StorageOptions options) => options
        .AddAny((p, valueType) =>
        {
            var implementationType = mongoProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers MongoDB historical storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoHistorical(this StorageOptions options, Type valueType)
    {
        options.HistoricalProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB historical storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoHistoricalAny(this StorageOptions options) => options
        .AddHistoricalAny((p, valueType) =>
        {
            var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers MongoDB partitioned storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoPartitioned(this StorageOptions options, Type valueType)
    {
        options.PartitionedProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB partitioned storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoPartitionedAny(this StorageOptions options) => options
        .AddPartitionedAny((p, valueType) =>
        {
            var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
}
