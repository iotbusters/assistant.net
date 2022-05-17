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
    ///     Registers the SQLite <paramref name="connectionString"/> to configure <see cref="SqliteOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, string connectionString) => services
        .Configure<SqliteOptions>(name, o => o.Connection(connectionString));

    /// <summary>
    ///     Registers a configuration instance which <see cref="SqliteOptions"/> will bind against.
    /// </summary>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<SqliteOptions>(name, configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string name, Action<SqliteOptions> configureOptions) => services
        .Configure(name, configureOptions);
}
