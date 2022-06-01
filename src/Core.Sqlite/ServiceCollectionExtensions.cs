using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net;

/// <summary>
///     Service collection extensions for SQLite providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the SQLite <paramref name="connectionString"/> to configure the same named <see cref="SqliteOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="connectionString">The SQLite connection string.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string connectionString) => services
        .ConfigureSqliteOptions(Microsoft.Extensions.Options.Options.DefaultName, o => o.Connection(connectionString));

    /// <summary>
    ///     Registers a configuration instance which the same named <see cref="SqliteOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureSqliteOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, Action<SqliteOptions> configureOptions) => services
        .ConfigureSqliteOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers the SQLite <paramref name="connectionString"/> to configure the same named <see cref="SqliteOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, string connectionString) => services
        .Configure<SqliteOptions>(name, o => o.Connection(connectionString));

    /// <summary>
    ///     Registers a configuration instance which the same named <see cref="SqliteOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<SqliteOptions>(name, configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, Action<SqliteOptions> configureOptions) => services
        .Configure(name, configureOptions);
}
