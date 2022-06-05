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
    /// <summary>
    ///     Configures local provider dependencies for storages.
    /// </summary>
    public static StorageBuilder UseMongoProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Configures storage to use a local single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageBuilder UseLocalSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.UseLocalSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Adds single provider based storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingle<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSingle(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds single provider based storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingle(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSingle(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds single provider based storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingleAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSingleAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingleHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSingleHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingleHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSingleHistorical(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSingleHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSingleHistoricalAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSinglePartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSinglePartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds local partitioned storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered providers.
    /// </remarks>
    public static StorageBuilder AddSinglePartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSinglePartitioned(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds local partitioned storage for any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder AddSinglePartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSinglePartitionedAny())
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocal(valueType))
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocalAny())
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocalHistorical(valueType))
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocalHistoricalAny())
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocalPartitioned(valueType))
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
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.AddLocalPartitionedAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Apply a configuration type <typeparamref name="TConfiguration"/>.
    /// </summary>
    public static StorageBuilder AddConfiguration<TConfiguration>(this StorageBuilder builder)
        where TConfiguration : IStorageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Apply a list of configuration instances <paramref name="storageConfigurations"/>.
    /// </summary>
    public static StorageBuilder AddConfiguration(this StorageBuilder builder, params IStorageConfiguration[] storageConfigurations)
    {
        foreach (var config in storageConfigurations)
            config.Configure(builder);
        return builder;
    }

    private static IServiceCollection AddLocalProvider(this IServiceCollection services) => services
        .TryAddSingleton(typeof(LocalStorageProvider<>), typeof(LocalStorageProvider<>))
        .TryAddSingleton(typeof(LocalHistoricalStorageProvider<>), typeof(LocalHistoricalStorageProvider<>))
        .TryAddSingleton(typeof(LocalPartitionedStorageProvider<>), typeof(LocalPartitionedStorageProvider<>));
}
