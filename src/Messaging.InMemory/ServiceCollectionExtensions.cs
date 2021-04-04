using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and default <see cref="CommandOptions"/> configuration.
        ///     Pay attention, you need to call explicitly <see cref="Options.ServiceCollectionExtensions.AddCommandOptions"/> to register handlers.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IHandlerFactory, HandlerFactory>();
            services.TryAddSingleton<ICommandClient, CommandClient>();
            return services
                .AddSystemServicesDefaulted()
                .AddCommandOptions(b => b.Add<DefaultInterceptorConfiguration>());
        }

        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and <see cref="CommandOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddCommandClient()
            .AddCommandOptions(configureOptions);
    }
}