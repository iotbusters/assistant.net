using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for SQLite based remote message handling on a server.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds system services with self-hosted service based behavior.
    /// </summary>
    public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
        .AddSystemServicesDefaulted()
        .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

    /// <summary>
    ///     Registers remote message handling server configuration.
    /// </summary>
    public static IServiceCollection AddSqliteMessageHandling(this IServiceCollection services, Action<SqliteHandlingBuilder> configureBuilder) => services
        .AddHostedService<SqliteMessageHandlingService>()
        .AddSystemServicesHosted()
        .AddMessagingClient()
        .AddStorage(b => b
            .AddSqlitePartitioned<int, IAbstractMessage>()
            .AddSqlite<int, long>())
        .ConfigureSqliteMessageHandling(b => b.AddConfiguration<SqliteServerInterceptorConfiguration>())
        .ConfigureSqliteMessageHandling(configureBuilder)
        .AddOptions<SqliteHandlingServerOptions>()
        .ChangeOn<MessagingClientOptions>(SqliteOptionsNames.DefaultName, (so, mo) =>
        {
            so.MessageTypes.Clear();
            foreach (var messageType in mo.Handlers.Keys)
                so.MessageTypes.Add(messageType);
        }).Services;

    /// <summary>
    ///     Configures remote message handling, required services and <see cref="SqliteHandlingServerOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureSqliteMessageHandling(this IServiceCollection services, Action<SqliteHandlingBuilder> configureBuilder)
    {
        var builder = new SqliteHandlingBuilder(services);
        configureBuilder(builder);
        return services;
    }

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureSqliteHandlingServerOptions(this IServiceCollection services, Action<SqliteHandlingServerOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureSqliteHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<SqliteHandlingServerOptions>(configuration);
}
