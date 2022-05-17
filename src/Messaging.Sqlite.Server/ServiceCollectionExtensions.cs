using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for MongoDb based remote message handling on a server.
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
    public static IServiceCollection AddMongoMessageHandling(this IServiceCollection services, Action<SqliteHandlingBuilder> configureBuilder) => services
        //.AddHostedService<MessageHandlingService>()
        .AddSystemServicesHosted()
        .AddMessagingClient()
        .ConfigureMongoMessageHandling(b => b.RemoveExposedException<OperationCanceledException>())
        .ConfigureMongoMessageHandling(configureBuilder)
        .AddOptions<SqliteHandlingServerOptions>()
        .ChangeOn<MessagingClientOptions>(SqliteOptionsNames.DefaultName)
        .Configure<IOptionsMonitor<MessagingClientOptions>>((o, m) =>
        {
            o.MessageTypes.Clear();
            foreach (var messageType in m.Get(SqliteOptionsNames.DefaultName).Handlers.Keys)
                o.MessageTypes.Add(messageType);
        }).Services;

    /// <summary>
    ///     Configures remote message handling, required services and <see cref="SqliteHandlingServerOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureMongoMessageHandling(this IServiceCollection services, Action<SqliteHandlingBuilder> configureBuilder)
    {
        var builder = new SqliteHandlingBuilder(services);
        configureBuilder(builder);
        return services;
    }

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, Action<SqliteHandlingServerOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="SqliteHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<SqliteHandlingServerOptions>(configuration);
}