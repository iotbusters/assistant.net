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
        ///     Pay attention, you need to call explicitly <see cref="ConfigureMessagingClient"/> to register handlers.
        /// </remarks>
        public static IServiceCollection AddMessagingClient(this IServiceCollection services) => services
            .AddDiagnostics()
            .AddSystemServicesDefaulted()
            .TryAddScoped<IMessagingClient, MessagingClient>()
            .ConfigureMessagingClient(b => b.AddConfiguration<DefaultInterceptorConfiguration>());

        /// <summary>
        ///     Adds <see cref="IMessagingClient"/> implementation, required services and <see cref="MessagingClientOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
            .AddMessagingClient()
            .ConfigureMessagingClient(configure);

        /// <summary>
        ///     Configures <see cref="IMessagingClient"/> implementation, required services and <see cref="MessagingClientOptions"/>.
        /// </summary>
        public static IServiceCollection ConfigureMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure)
        {
            configure(new MessagingClientBuilder(services));
            return services;
        }

        /// <summary>
        ///     Register an action used to configure the same named <see cref="MessagingClientOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, Action<MessagingClientOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}
