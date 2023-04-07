using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring SQLite storages.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures storage to use SQLite storage provider implementation.
    /// </summary>
    public static StorageBuilder UseSqlite(this StorageBuilder builder) => builder
        .UseSqlite(delegate { });

    /// <summary>
    ///     Configures storage to use SQLite storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The SQLite connection string.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures storage to use SQLite storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services
            .ConfigureSqliteOptions(builder.Name, configureOptions)
            .AddSqliteProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures storage to use SQLite storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static StorageBuilder UseSqlite(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .ConfigureSqliteOptions(builder.Name, configuration)
            .AddSqliteProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures specific DbContest option builder to connect SQLite database.
    /// </summary>
    internal static TBuilder UseSqlite<TBuilder>(this TBuilder builder, SqliteOptions options) where TBuilder : DbContextOptionsBuilder
    {
        if (options.Connection != null)
            builder.UseSqlite(options.Connection);
        else
            builder.UseSqlite(options.ConnectionString);
        return builder;
    }

    /// <summary>
    ///     Configures SQLite single provider dependencies for storages.
    /// </summary>
    private static void AddSqliteProvider(this IServiceCollection services, string name) => services
        .ConfigureStorageOptions(name, o => o.UseSqlite())
        .TryAddScoped(typeof(SqliteStorageProvider<>), typeof(SqliteStorageProvider<>))
        .TryAddScoped(typeof(SqliteHistoricalStorageProvider<>), typeof(SqliteHistoricalStorageProvider<>))
        .TryAddScoped(typeof(SqlitePartitionedStorageProvider<>), typeof(SqlitePartitionedStorageProvider<>))
        .TryAddSingleton<IPostConfigureOptions<SqliteOptions>, SqlitePostConfigureOptions>()
        .AddDbContextFactory<StorageDbContext>((p, b) =>
        {
            var options = p.GetRequiredService<INamedOptions<SqliteOptions>>().Value;
            b.UseSqlite(options);
        }, lifetime: ServiceLifetime.Scoped);
}
