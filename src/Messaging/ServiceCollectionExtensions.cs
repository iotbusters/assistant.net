using Assistant.Net.Analyzers;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and defaults.
        ///     Pay attention, you need to call explicitly <see cref="ConfigureCommandClient"/> to register handlers.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services) => services
            .AddStorage(b => b.AddLocal<object, CachingResult>())
            .AddDiagnostics()
            .AddSystemServicesDefaulted()
            .AddProxyFactory(b => b.Add<IAbstractHandler>())
            .TryAddSingleton<IHandlerFactory, HandlerFactory>()
            .TryAddSingleton<ICommandClient, CommandClient>()
            .ConfigureCommandClient(b => b.AddConfiguration<DefaultInterceptorConfiguration>());

        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and <see cref="CommandClientOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services, Action<CommandClientBuilder> configure) => services
            .AddCommandClient()
            .ConfigureCommandClient(configure);

        /// <summary>
        ///     Register an action used to configure <see cref="CommandClientOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureCommandClient(this IServiceCollection services, Action<CommandClientBuilder> configure)
        {
            configure(new CommandClientBuilder(services));
            return services;
        }

        /// <summary>
        ///     Register an action used to configure the same named <see cref="CommandClientOptions"/> options.
        /// </summary>
        internal static IServiceCollection ConfigureCommandClientOptions(this IServiceCollection services, Action<CommandClientOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}