using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Configuration;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds <see cref="ICommandClient"/> implementation, required services and default <see cref="CommandOptions"/> configuration.
        ///     Pay attention, you need to call explicitly <see cref="Assistant.Net.Messaging.ServiceCollectionExtensions.AddCommandOptions"/> to register handlers.
        /// </summary>
        public static IServiceCollection AddCommandClient(this IServiceCollection services) => services
            // todo: use persisted caching. https://github.com/iotbusters/assistant.net/issues/23
            .AddStorage(b => b.AddLocal<CachingResult>())
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

        /// <summary>
        ///     Register an action used to configure <see cref="CommandOptions"/> options.
        /// </summary>
        public static IServiceCollection AddCommandOptions(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddCommandOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        /// <summary>
        ///     Register an action used to configure the same named <see cref="CommandOptions"/> options.
        /// </summary>
        public static IServiceCollection AddCommandOptions(this IServiceCollection services, string name, Action<CommandOptions> configureOptions) => services
            .Configure(name, configureOptions);
    }
}