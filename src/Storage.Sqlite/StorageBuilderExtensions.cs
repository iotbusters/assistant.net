using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring SQLite storages.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures SQLite provider dependencies for storages.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseSqlite{TBuilder}(IBuilder{TBuilder},string)"/> to configure;
    ///     It should be added if <see cref="AddSqlite"/> wasn't configured on the start.
    /// </remarks>
    public static StorageBuilder UseSqliteProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddStorageDbContext()
            .ConfigureSqliteStoringOptions(builder.Name, delegate { });
        return builder;
    }
    /// <summary>
    ///     Configures storage to use an SQLite single provider implementation.
    /// </summary>
    public static StorageBuilder UseSqliteSingleProvider(this StorageBuilder builder)
    {
        builder.Services
            .AddStorageDbContext()
            .ConfigureStorageOptions(builder.Name, o => o.UseSqliteSingleProvider());
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
            .AddStorageDbContext()
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
            .AddStorageDbContext()
            .ConfigureStorageOptions(builder.Name, o => o.AddSqliteAny())
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
            .AddStorageDbContext()
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
            .AddStorageDbContext()
            .ConfigureStorageOptions(builder.Name, o => o.AddSqliteHistoricalAny())
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
            .AddStorageDbContext()
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
            .AddStorageDbContext()
            .ConfigureStorageOptions(builder.Name, o => o.AddSqlitePartitionedAny())
            .ConfigureSerializer(builder.Name, b => b.AddJsonTypeAny());
        return builder;
    }

    /// <summary>
    ///     
    /// </summary>
    public static IServiceCollection AddStorageDbContext(this IServiceCollection services) => services
        .AddNamedOptionsContext()
        .AddDbContextFactory<StorageDbContext>((p, b) =>
        {
            var options = p.GetRequiredService<INamedOptions<SqliteOptions>>().Value;
            if (options.Connection != null)
                b.UseSqlite(options.Connection);
            else
                b.UseSqlite(options.ConnectionString);
        }, lifetime: ServiceLifetime.Scoped);
}
