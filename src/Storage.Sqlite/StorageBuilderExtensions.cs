using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring SQLite storages.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures storage to use SQLite single provider.
    /// </summary>
    public static StorageBuilder UseSqliteSingleProvider(this StorageBuilder builder)
    {
        builder.Services.AddSqliteSingleProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures storage to use SQLite provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The SQLite connection string.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures storage to use SQLite provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services
            .ConfigureSqliteOptions(builder.Name, configureOptions)
            .AddSqliteProvider();
        return builder;
    }

    /// <summary>
    ///     Configures storage to use SQLite provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .ConfigureSqliteOptions(builder.Name, configuration)
            .AddSqliteProvider();
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddSqlite<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqlite(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddSqlite(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqlite(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddSqliteAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqliteAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
    ///     including value change history.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddSqliteHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqliteHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>
    ///     including value change history.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddSqliteHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqliteHistorical(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage for any unregistered type including value change history.
    /// </summary>
    public static StorageBuilder AddSqliteHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqliteHistoricalAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds partitioned SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    /// <typeparam name="TKey">The key type to configure a storage.</typeparam>
    /// <typeparam name="TValue">The value type to configure a storage.</typeparam>
    public static StorageBuilder AddSqlitePartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqlitePartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds partitioned SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="keyType">The key type to configure a storage.</param>
    /// <param name="valueType">The value type to configure a storage.</param>
    public static StorageBuilder AddSqlitePartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqlitePartitioned(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds partitioned SQLite storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddSqlitePartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddSqlitePartitionedAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Configures SQLite regular provider dependencies for storages.
    /// </summary>
    private static void AddSqliteProvider(this IServiceCollection services) => services
        .TryAddScoped(typeof(SqliteStorageProvider<>), typeof(SqliteStorageProvider<>))
        .TryAddScoped(typeof(SqliteHistoricalStorageProvider<>), typeof(SqliteHistoricalStorageProvider<>))
        .TryAddScoped(typeof(SqlitePartitionedStorageProvider<>), typeof(SqlitePartitionedStorageProvider<>))
        .AddDbContextFactory<StorageDbContext>((p, b) =>
        {
            var options = p.GetRequiredService<INamedOptions<SqliteOptions>>().Value;
            if (options.Connection != null)
                b.UseSqlite(options.Connection);
            else
                b.UseSqlite(options.ConnectionString);
        }, lifetime: ServiceLifetime.Scoped);

    /// <summary>
    ///     Configures SQLite single provider dependencies for storages.
    /// </summary>
    private static void AddSqliteSingleProvider(this IServiceCollection services, string name) => services
        .ConfigureStorageOptions(name, o => o.UseSqliteSingleProvider())
        .AddSqliteProvider();
}
