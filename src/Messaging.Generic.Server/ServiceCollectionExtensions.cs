using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for server message handling.
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
    ///     Registers storage based server message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it's configured to use storage single provider required only <i>Use{Provider}</i>.
    /// </remarks>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services) => services
        .AddHostedService<GenericMessageHandlingService>()
        .AddScoped<MessageHandler>()
        .AddSystemServicesHosted()
        .AddMessagingClient(GenericOptionsNames.DefaultName, b => b.AddConfiguration<GenericServerInterceptorConfiguration>())
        .AddOptions<GenericHandlingServerOptions>()
        .ChangeOn<MessagingClientOptions>(GenericOptionsNames.DefaultName, (so, mo) =>
        {
            so.MessageTypes.Clear();
            foreach (var messageType in mo.Handlers.Keys)
                so.MessageTypes.Add(messageType);
        }).Services;

    /// <summary>
    ///     Registers storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the <see cref="GenericOptionsNames.DefaultName"/> named messaging client.</param>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services, Action<GenericHandlingServerBuilder> configure) => services
        .AddGenericMessageHandling()
        .ConfigureGenericMessageHandling(configure);

    /// <summary>
    ///     Configures the <see cref="GenericOptionsNames.DefaultName"/> named messaging client.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureBuilder">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureGenericMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configureBuilder) => services
        .ConfigureMessagingClient(GenericOptionsNames.DefaultName, configureBuilder);

    /// <summary>
    ///     Configures storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureBuilder">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureGenericMessageHandling(this IServiceCollection services, Action<GenericHandlingServerBuilder> configureBuilder)
    {
        var builder = new GenericHandlingServerBuilder(services);
        configureBuilder(builder);
        return services;
    }

    /// <summary>
    ///    Register an action used to configure default <see cref="GenericHandlingServerOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureGenericHandlingServerOptions(this IServiceCollection services, Action<GenericHandlingServerOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///     Registers a configuration instance which default <see cref="GenericHandlingServerOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureGenericHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<GenericHandlingServerOptions>(configuration);
}
