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
    public static IServiceCollection AddMongoMessageHandling(this IServiceCollection services, Action<MessagingClientBuilder> configureBuilder) => services
        .AddHostedService<MongoMessageHandlingService>()
        .AddSystemServicesHosted()
        .AddMessagingClient()
        .AddStorage(MongoOptionsNames.DefaultName, b => b
            .AddMongoPartitioned<int, IAbstractMessage>()
            .AddMongo<int, long>())
        .ConfigureMongoMessageHandling(b => b.AddConfiguration<MongoServerInterceptorConfiguration>())
        .ConfigureMongoMessageHandling(configureBuilder)
        .AddOptions<MongoHandlingServerOptions>()
        .ChangeOn<MessagingClientOptions>(MongoOptionsNames.DefaultName, (so, mo) =>
        {
            so.MessageTypes.Clear();
            foreach (var messageType in mo.Handlers.Keys)
                so.MessageTypes.Add(messageType);
        }).Services;

    /// <summary>
    ///     Configures remote message handling, required services and <see cref="MongoHandlingServerOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureMongoMessageHandling(this IServiceCollection services, Action<MessagingClientBuilder> configureBuilder)
    {
        var builder = new MessagingClientBuilder(services, MongoOptionsNames.DefaultName);
        configureBuilder(builder);
        return services;
    }

    /// <summary>
    ///    Register an action used to configure <see cref="MongoHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, Action<MongoHandlingServerOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="MongoHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<MongoHandlingServerOptions>(configuration);
}
