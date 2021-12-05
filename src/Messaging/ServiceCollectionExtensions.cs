using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging
{
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
            .AddDiagnostics()
            .AddSystemServicesDefaulted()
            .TryAddSingleton<IMessagingClientFactory, MessagingClientFactory>()
            .TryAddSingleton(p => p.GetRequiredService<IMessagingClientFactory>().Create())
            .ConfigureMessagingClient(b => b.AddConfiguration<DefaultInterceptorConfiguration>());

        /// <summary>
        ///     Adds <see cref="IMessagingClient"/> implementation, required services and options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configure">The action used to configure the default messaging client.</param>
        public static IServiceCollection AddMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
            .AddMessagingClient()
            .ConfigureMessagingClient(configure);

        /// <summary>
        ///     Configures <see cref="IMessagingClient"/> instance, required services and options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configure">The action used to configure the default messaging client.</param>
        public static IServiceCollection ConfigureMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure)
        {
            configure(new MessagingClientBuilder(Microsoft.Extensions.Options.Options.DefaultName, services));
            return services;
        }

        /// <summary>
        ///     Configures <see cref="IMessagingClient"/> implementation, required services and options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="name">The name of the <see cref="MessagingClientOptions"/> instance.</param>
        /// <param name="configure">The action used to configure the named <see cref="MessagingClientOptions"/> instance.</param>
        public static IServiceCollection ConfigureMessagingClient(this IServiceCollection services, string name, Action<MessagingClientBuilder> configure)
        {
            configure(new MessagingClientBuilder(name, services));
            return services;
        }

        /// <summary>
        ///     Register an action used to configure the same named <see cref="MessagingClientOptions"/> options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        public static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, Action<MessagingClientOptions> configureOptions) => services
            .Configure(configureOptions);

        /// <summary>
        ///     Register an action used to configure the same named <see cref="MessagingClientOptions"/> options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        public static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, string name, Action<MessagingClientOptions> configureOptions) => services
            .Configure(name, configureOptions);
    }
}
