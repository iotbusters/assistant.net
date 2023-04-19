using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for server message handling.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers storage based server message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it's configured to use storage single provider required only <i>Use{Provider}</i>.
    /// </remarks>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services) => services
        .AddGenericMessageHandling(Microsoft.Extensions.Options.Options.DefaultName);

    /// <summary>
    ///     Registers storage based server message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it's configured to use storage single provider required only <i>Use{Provider}</i>.
    /// </remarks>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services, string name) => services
        .AddHostedService(p => p.Create<GenericMessageHandlingService>(name))
        .AddSystemServicesHosted()
        .AddMessagingClient(name, b => b.AddConfiguration<GenericServerInterceptorConfiguration>())
        .AddHealthChecks().Services
        .Configure<HealthCheckPublisherOptions>(o =>
        {
            o.Delay = TimeSpan.Zero;
            o.Period = TimeSpan.FromSeconds(10);
            o.Timeout = TimeSpan.FromSeconds(3);
        })
        .TryAddSingletonEnumerable<IHealthCheckPublisher, ServerActivityPublisher>()
        .TryAddSingletonEnumerable<IHealthCheckPublisher, ServerAvailabilityPublisher>()
        .TryAddSingleton<ServerAvailabilityService>()
        .TryAddSingleton<ServerActivityService>();

    /// <summary>
    ///     Registers storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureBuilder">The action used to configure a default messaging client.</param>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services, Action<GenericHandlingServerBuilder> configureBuilder) => services
        .AddGenericMessageHandling(Microsoft.Extensions.Options.Options.DefaultName, configureBuilder);

    /// <summary>
    ///     Registers storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configureBuilder">The action used to configure the named messaging client.</param>
    public static IServiceCollection AddGenericMessageHandling(this IServiceCollection services, string name, Action<GenericHandlingServerBuilder> configureBuilder) => services
        .AddGenericMessageHandling(name)
        .ConfigureGenericMessageHandling(name, configureBuilder);

    /// <summary>
    ///     Configures storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureBuilder">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureGenericMessageHandling(this IServiceCollection services, Action<GenericHandlingServerBuilder> configureBuilder) => services
        .ConfigureGenericMessageHandling(Microsoft.Extensions.Options.Options.DefaultName, configureBuilder);

    /// <summary>
    ///     Configures storage based server message handling.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configureBuilder">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureGenericMessageHandling(this IServiceCollection services, string name, Action<GenericHandlingServerBuilder> configureBuilder)
    {
        var builder = new GenericHandlingServerBuilder(services, name);
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
    ///    Register an action used to configure default <see cref="GenericHandlingServerOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureGenericHandlingServerOptions(this IServiceCollection services, string name, Action<GenericHandlingServerOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Registers a configuration instance which default <see cref="GenericHandlingServerOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureGenericHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<GenericHandlingServerOptions>(configuration);

    /// <summary>
    ///     Registers a configuration instance which default <see cref="GenericHandlingServerOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureGenericHandlingServerOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<GenericHandlingServerOptions>(name, configuration);
}
