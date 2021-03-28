using System;
using System.Linq;
using System.Threading;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Assistant.Net.Messaging.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandClient(this IServiceCollection services, Action<CommandConfigurationBuilder> configure)
        {
            services
                .AddSystemLifetime(p => CancellationToken.None)
                .AddCommandOptions(configure);
            services.TryAddSingleton<IHandlerFactory, HandlerFactory>();
            services.TryAddSingleton<ICommandClient, CommandClient>();
            return services;
        }

        public static IServiceCollection AddCommandOptions(this IServiceCollection services, Action<CommandConfigurationBuilder> configure)
        {
            var builder = new CommandConfigurationBuilder();
            configure?.Invoke(builder);

            var interceptors = builder.Interceptors.Distinct();
            var handlers = builder.Handlers.Distinct();

            foreach (var implementation in handlers)
                services.TryAddTransient(implementation);

            foreach (var implementation in interceptors)
                services.TryAddTransient(implementation);

            return services.Configure<CommandOptions>(opt =>
            {
                opt.Handlers.AddRange(handlers);
                opt.Interceptors.AddRange(interceptors);
            });
        }
    }
}