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
    ///     Registers a configuration instance which <see cref="SqliteStoringOptions"/> will bind against.
    /// </summary>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<SqliteStoringOptions>(configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteStoringOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureSqliteStoringOptions(this IServiceCollection services, Action<SqliteStoringOptions> configureOptions) => services
        .Configure(configureOptions);
}
