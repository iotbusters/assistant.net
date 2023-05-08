using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for message handling client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds <see cref="IMessagingClient"/> implementation, required services and defaults.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly <see cref="ConfigureMessagingClient(IServiceCollection,Action{MessagingClientBuilder})"/> to register handlers.
    /// </remarks>
    public static IServiceCollection AddMessagingClient(this IServiceCollection services) => services
        .AddMessagingClient(delegate { });

    /// <summary>
    ///     Adds <see cref="IMessagingClient"/> implementation, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the default messaging client.</param>
    public static IServiceCollection AddMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
        .AddMessagingClient(Microsoft.Extensions.Options.Options.DefaultName, configure);

    /// <summary>
    ///     Adds <see cref="IMessagingClient"/> implementation, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configure">The action used to configure the default messaging client.</param>
    public static IServiceCollection AddMessagingClient(this IServiceCollection services, string name, Action<MessagingClientBuilder> configure) => services
        .AddDiagnostics()
        .AddNamedOptionsContext()
        .AddSystemServicesDefaulted()
        .TryAddScoped<IMessagingClient, MessagingClient>()
        .ConfigureJsonSerialization(name)
        .ConfigureMessagingClient(name, b => b.AddConfiguration<DefaultInterceptorConfiguration>())
        .ConfigureMessagingClient(name, configure);

    /// <summary>
    ///     Configures <see cref="IMessagingClient"/> instance, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the default messaging client.</param>
    public static IServiceCollection ConfigureMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
        .ConfigureMessagingClient(Microsoft.Extensions.Options.Options.DefaultName, configure);

    /// <summary>
    ///     Configures <see cref="IMessagingClient"/> implementation, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of related option instances.</param>
    /// <param name="configure">The action used to configure the named option instances.</param>
    public static IServiceCollection ConfigureMessagingClient(this IServiceCollection services, string name, Action<MessagingClientBuilder> configure)
    {
        configure(new(services, name));
        return services;
    }

    /// <summary>
    ///     Register an action used to configure default <see cref="MessagingClientOptions"/> options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, Action<MessagingClientOptions> configureOptions) => services
        .ConfigureMessagingClientOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="MessagingClientOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, string name, Action<MessagingClientOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Registers default <see cref="ISerializer{TValue}"/> configuration.
    /// </summary>
    /// <remarks>
    ///     Only <see cref="MessagingClientOptions.ExposedExceptions"/> are being serialized.
    /// </remarks>
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services) => services
        .ConfigureJsonSerialization(Microsoft.Extensions.Options.Options.DefaultName);

    /// <summary>
    ///     Registers named <see cref="ISerializer{TValue}"/> configuration.
    /// </summary>
    /// <remarks>
    ///     Only <see cref="MessagingClientOptions.ExposedExceptions"/> are being serialized.
    /// </remarks>
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services, string name) => services
        .AddSerializer(name, b => b
            .UseJson()
            .AddJsonConverter<MessageExceptionJsonConverter>()
            .AllowAnyType());
}
