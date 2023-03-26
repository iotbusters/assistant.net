using Assistant.Net.Options;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Service collection extensions for SQLite storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the SQLite <paramref name="connectionString"/> to configure the same named <see cref="SqliteOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="connectionString">The SQLite connection string.</param>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string connectionString) => services
        .ConfigureSqliteOptions(Microsoft.Extensions.Options.Options.DefaultName, connectionString);

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
        .ConfigureSqliteOptions(name, o => o.Connection(connectionString));

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

    /// <summary>
    ///     Registers a configuration instance which default <see cref="SqliteStoringOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureSqliteStoringOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure default <see cref="SqliteStoringOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, Action<SqliteStoringOptions> configureOptions) => services
        .ConfigureSqliteStoringOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers a configuration instance which the same named <see cref="SqliteStoringOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<SqliteStoringOptions>(name, configuration);

    /// <summary>
    ///    Register an action used to configure the same named <see cref="SqliteStoringOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, string name, Action<SqliteStoringOptions> configureOptions) => services
        .Configure(name, configureOptions);
}
