using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Microsoft.Data.Sqlite;
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
    private static readonly Type sqliteProviderType = typeof(SqliteStorageProvider<>);
    private static readonly Type sqliteHistoricalProviderType = typeof(SqliteHistoricalStorageProvider<>);
    private static readonly Type sqlitePartitionedProviderType = typeof(SqlitePartitionedStorageProvider<>);

    /// <summary>
    ///     Configures storage to use an SQLite single provider implementation.
    /// </summary>
    public static StorageBuilder UseSqliteSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteProviderType, sqliteProviderType)
            .TryAddScoped(sqliteHistoricalProviderType, sqliteHistoricalProviderType)
            .TryAddScoped(sqlitePartitionedProviderType, sqlitePartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
            {
                o.SingleProvider = new((p, valueType) =>
                {
                    var implementationType = sqliteProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SingleHistoricalProvider = new((p, valueType) =>
                {
                    var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
                o.SinglePartitionedProvider = new((p, valueType) =>
                {
                    var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                });
            });
        return builder;
    }

    /// <summary>
    ///     Configures the storage to connect a SQLite database using <paramref name="connection"/>.
    /// </summary>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, SqliteConnection connection)
    {
        builder.Services
            .AddDbContext()
            .ConfigureSqliteOptions(builder.Name, o => o.Connection(connection))
            .ConfigureSqliteStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Configures the storage to connect a SQLite database by <paramref name="connectionString"/>.
    /// </summary>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures the storage to connect a SQLite database.
    /// </summary>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services
            .AddDbContext()
            .ConfigureSqliteOptions(builder.Name, configureOptions)
            .ConfigureSqliteStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Configures the storage to connect a SQLite database.
    /// </summary>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .AddDbContext()
            .ConfigureSqliteOptions(builder.Name, configuration)
            .ConfigureSqliteStoringOptions(builder.Name, delegate { });
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    public static StorageBuilder AddSqlite<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqlite(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    public static StorageBuilder AddSqlite(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteProviderType, sqliteProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.Providers[valueType] = new(p =>
                {
                    var implementationType = sqliteProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddSqliteAny(this StorageBuilder builder)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteProviderType, sqliteProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.ProviderAny = new((p, valueType) =>
                {
                    var implementationType = sqliteProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
    ///     including value change history.
    /// </summary>
    public static StorageBuilder AddSqliteHistorical<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqliteHistorical(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>
    ///     including value change history.
    /// </summary>
    public static StorageBuilder AddSqliteHistorical(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteHistoricalProviderType, sqliteHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviders[valueType] = new(p =>
                {
                    var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds SQLite storage for any unregistered type including value change history.
    /// </summary>
    public static StorageBuilder AddSqliteHistoricalAny(this StorageBuilder builder)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteHistoricalProviderType, sqliteHistoricalProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.HistoricalProviderAny = new((p, valueType) =>
                {
                    var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     Adds partitioned SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
    /// </summary>
    public static StorageBuilder AddSqlitePartitioned<TKey, TValue>(this StorageBuilder builder) => builder
        .AddSqlitePartitioned(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds partitioned SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
    /// </summary>
    public static StorageBuilder AddSqlitePartitioned(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqliteHistoricalProviderType, sqliteHistoricalProviderType)
            .TryAddScoped(sqlitePartitionedProviderType, sqlitePartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviders[valueType] = new(p =>
                {
                    var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonType(keyType).AddJsonType(valueType));
        return builder;
    }

    /// <summary>
    ///     Adds partitioned SQLite storage for any unregistered type.
    /// </summary>
    public static StorageBuilder AddSqlitePartitionedAny(this StorageBuilder builder)
    {
        builder.Services
            .AddDbContext()
            .TryAddScoped(sqlitePartitionedProviderType, sqlitePartitionedProviderType)
            .ConfigureStorageOptions(builder.Name, o =>
                o.PartitionedProviderAny = new((p, valueType) =>
                {
                    var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
                    return p.GetRequiredService(implementationType);
                }))
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    private static IServiceCollection AddDbContext(this IServiceCollection services) => services
        .AddDbContextFactory<StorageDbContext>((p, b) =>
        {
            var options = p.GetRequiredService<INamedOptions<SqliteOptions>>().Value;
            if (options.Connection != null)
                b.UseSqlite(options.Connection);
            else
                b.UseSqlite(options.ConnectionString);
        }, lifetime: ServiceLifetime.Scoped);
}
