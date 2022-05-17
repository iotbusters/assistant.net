using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for SQLite based remote message handling on a client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingClientOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingClientOptions(this IServiceCollection services, Action<SqliteHandlingClientOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingClientOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingClientOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<SqliteHandlingClientOptions>(configuration);
}