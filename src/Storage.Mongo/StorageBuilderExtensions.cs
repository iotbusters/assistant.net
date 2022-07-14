using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring MongoDB provider.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures storage to use MongoDB single provider.
    /// </summary>
    public static StorageBuilder UseMongoSingleProvider(this StorageBuilder builder)
    {
        builder.Services.AddMongoSingleProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures storage to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, string connectionString) => builder
        .UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures storage to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .ConfigureMongoOptions(builder.Name, configureOptions)
            .AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures storage to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .ConfigureMongoOptions(builder.Name, configuration)
            .AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddMongo<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongo(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddMongo(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongo(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddMongoAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
    ///     including value change history.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddMongoHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongoHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>
    ///     including value change history.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddMongoHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoHistorical(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds MongoDB storage for any unregistered type including value change history.
    /// </summary>
    public static StorageBuilder AddMongoHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoHistoricalAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder.AddMongoAny();
    }

    /// <summary>
    ///     Adds partitioned MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddMongoPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddMongoPartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds partitioned MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddMongoPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoPartitioned(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds partitioned MongoDB storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddMongoPartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoPartitionedAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Configures MongoDB provider dependencies for storages.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddMongoProvider(this IServiceCollection services, string name) => services
        .TryAddScoped(typeof(MongoStorageProvider<>), typeof(MongoStorageProvider<>))
        .TryAddScoped(typeof(MongoHistoricalStorageProvider<>), typeof(MongoHistoricalStorageProvider<>))
        .TryAddScoped(typeof(MongoPartitionedStorageProvider<>), typeof(MongoPartitionedStorageProvider<>))
        .TryAddSingleton<IPostConfigureOptions<MongoOptions>, MongoPostConfigureOptions>()
        .ConfigureMongoOptions(name, o => o.DatabaseName ??= MongoNames.DatabaseName)
        .AddMongoClient();

    /// <summary>
    ///     Configures MongoDB provider dependencies for storages.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddMongoSingleProvider(this IServiceCollection services, string name) => services
        .ConfigureStorageOptions(name, o => o.UseMongoSingleProvider())
        .AddMongoProvider(name);
}
