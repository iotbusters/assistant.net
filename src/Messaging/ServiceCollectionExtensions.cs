using Assistant.Net.Diagnostics;
using Assistant.Net.Dynamics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
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
        ///     Pay attention, you need to call explicitly <see cref="ConfigureMessageClient"/> to register handlers.
        /// </summary>
        public static IServiceCollection AddMessagingClient(this IServiceCollection services) => services
            .AddStorage(b => b.AddLocal<object, CachingResult>())
            .AddDiagnostics()
            .AddSystemServicesDefaulted()
            .AddProxyFactory(b => b.Add<IAbstractHandler>())
            .TryAddSingleton<IMessagingClient, MessagingClient>()
            .ConfigureMessageClient(b => b.AddConfiguration<DefaultInterceptorConfiguration>());

        /// <summary>
        ///     Adds <see cref="IMessagingClient"/> implementation, required services and <see cref="MessagingClientOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddMessagingClient(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
            .AddMessagingClient()
            .ConfigureMessageClient(configure);

        /// <summary>
        ///     Register an action used to configure <see cref="MessagingClientOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMessageClient(this IServiceCollection services, Action<MessagingClientBuilder> configure)
        {
            configure(new MessagingClientBuilder(services));
            return services;
        }

        /// <summary>
        ///     Register an action used to configure the same named <see cref="MessagingClientOptions"/> options.
        /// </summary>
        internal static IServiceCollection ConfigureMessagingClientOptions(this IServiceCollection services, Action<MessagingClientOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}