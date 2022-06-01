using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring MongoDB storages.
/// </summary>
public static class StorageBuilderExtensions
{
    private static readonly Type mongoProviderType = typeof(MongoStorageProvider<>);
    private static readonly Type mongoHistoricalProviderType = typeof(MongoHistoricalStorageProvider<>);
    private static readonly Type mongoPartitionedProviderType = typeof(MongoPartitionedStorageProvider<>);

    /// <summary>
    ///     Configures storage to use a MongoDB single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseMongo(StorageBuilder,string)"/> to configure.
    /// </remarks>
    public static StorageBuilder UseMongoSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoProviderType, mongoProviderType)
            .TryAddScoped(mongoHistoricalProviderType, mongoHistoricalProviderType)
            .TryAddScoped(mongoPartitionedProviderType, mongoPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
            {
                o.SingleProvider = new((p, valueType) =>
                {
                    var implementationType = mongoProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SingleHistoricalProvider = new((p, valueType) =>
                {
                    var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SinglePartitionedProvider = new((p, valueType) =>
                {
                    var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
            });
        return builder;
    }

    /// <summary>
    ///     Configures the storage to connect a MongoDB database by <paramref name="connectionString"/>.
    /// </summary>
    public static StorageBuilder UseMongo(this StorageBuilder builder, string connectionString) =>
        builder.UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the storage to connect a MongoDB database.
    /// </summary>
    public static StorageBuilder UseMongo(this StorageBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .AddMongoClient()
            .ConfigureMongoOptions(builder.Name, o => o.Database(MongoNames.DatabaseName))
            .ConfigureMongoOptions(builder.Name, configureOptions)
            .ConfigureMongoStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Configures the storage to connect a MongoDB database.
    /// </summary>
    public static StorageBuilder UseMongo(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .AddMongoClient()
            .ConfigureMongoOptions(builder.Name, o => o.Database(MongoNames.DatabaseName))
            .ConfigureMongoOptions(builder.Name, configuration)
            .ConfigureMongoStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    public static StorageBuilder AddMongo<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongo(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    public static StorageBuilder AddMongo(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoProviderType, mongoProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.Providers[valueType] = new(p =>
                {
                    var implementationType = mongoProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddMongoAny(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoProviderType, mongoProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.ProviderAny = new((p, valueType) =>
                {
                    var implementationType = mongoProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
    ///     including value change history.
    /// </summary>
    public static StorageBuilder AddMongoHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongoHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>
    ///     including value change history.
    /// </summary>
    public static StorageBuilder AddMongoHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoHistoricalProviderType, mongoHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviders[valueType] = new(p =>
                {
                    var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage for any unregistered type
    ///     including value change history.
    /// </summary>
    public static StorageBuilder AddMongoHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoHistoricalProviderType, mongoHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviderAny = new((p, valueType) =>
                {
                    var implementationType = mongoHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder.AddMongoAny();
    }

    /// <summary>
    ///     Adds partitioned MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    public static StorageBuilder AddMongoPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongoPartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds partitioned MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    public static StorageBuilder AddMongoPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoHistoricalProviderType, mongoHistoricalProviderType)
            .TryAddScoped(mongoPartitionedProviderType, mongoPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviders[valueType] = new(p =>
                {
                    var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds partitioned MongoDB storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddMongoPartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoClient()
            .TryAddScoped(mongoPartitionedProviderType, mongoPartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviderAny = new((p, valueType) =>
                {
                    var implementationType = mongoPartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }
}
