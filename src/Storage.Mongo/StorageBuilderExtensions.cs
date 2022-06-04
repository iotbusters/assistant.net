using Assistant.Net.Abstractions;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring MongoDB storages.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures MongoDB provider dependencies for storages.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseMongo{TBuilder}(IBuilder{TBuilder},string)"/> to configure;
    ///     It should be added if <see cref="AddMongo"/> wasn't configured on the start.
    /// </remarks>
    public static StorageBuilder UseMongoProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoProvider(builder.Name)
            .ConfigureMongoStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Configures storage to use a MongoDB single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseMongo{TBuilder}(IBuilder{TBuilder},string)"/> to configure.
    /// </remarks>
    public static StorageBuilder UseMongoSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddMongoProvider(builder.Name)
            .ConfigureStorageOptions(builder.Name, o => o.UseMongoSingleProvider());
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
            .AddMongoProvider(builder.Name)
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
            .AddMongoProvider(builder.Name)
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoAny())
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
            .AddMongoProvider(builder.Name)
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoHistorical(valueType))
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
            .AddMongoProvider(builder.Name)
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoHistoricalAny())
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
            .AddMongoProvider(builder.Name)
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
            .AddMongoProvider(builder.Name)
            .ConfigureStorageOptions(builder.Name, o => o.AddMongoPartitionedAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    private static IServiceCollection AddMongoProvider(this IServiceCollection services, string name) => services
        .ConfigureMongoOptions(name, o => o.DatabaseName ??= MongoNames.DatabaseName)
        .AddMongoClient();
}
