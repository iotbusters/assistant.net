using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Diagnostics;
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
        ///     Pay attention, you need to call explicitly <see cref="Assistant.Net.Messaging.ServiceCollectionExtensions.AddCommandOptions"/> to register handlers.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services) => services
            .AddDiagnostics()
            .AddSystemServicesDefaulted()
            .TryAddSingleton<IHandlerFactory, HandlerFactory>()
            .TryAddSingleton<ICommandClient, CommandClient>()
            .AddCommandOptions(b => b.Add<DefaultInterceptorConfiguration>());

        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and <see cref="CommandOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddCommandClient()
            .AddCommandOptions(configureOptions);
    }
}