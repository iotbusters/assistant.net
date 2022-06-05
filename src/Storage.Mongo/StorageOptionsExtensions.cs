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
    public static StorageOptions UseMongoSingleProvider(this StorageOptions options)
    {
        options.SingleProvider = new((p, valueType) =>
        {
            var implementationType = mongoProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        options.SingleHistoricalProvider = new((p, valueType) =>
        {
            var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        options.SinglePartitionedProvider = new((p, valueType) =>
        {
            var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongo(this StorageOptions options, Type valueType)
    {
        options.Providers[valueType] = new(p =>
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
    public static StorageOptions AddMongoAny(this StorageOptions options)
    {
        options.ProviderAny = new((p, valueType) =>
        {
            var implementationType = mongoProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB historical storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoHistorical(this StorageOptions options, Type valueType)
    {
        options.HistoricalProviders[valueType] = new(p =>
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
    public static StorageOptions AddMongoHistoricalAny(this StorageOptions options)
    {
        options.HistoricalProviderAny = new((p, valueType) =>
        {
            var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers MongoDB partitioned storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddMongoPartitioned(this StorageOptions options, Type valueType)
    {
        options.PartitionedProviders[valueType] = new(p =>
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
    public static StorageOptions AddMongoPartitionedAny(this StorageOptions options)
    {
        options.PartitionedProviderAny = new((p, valueType) =>
        {
            var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }
}
