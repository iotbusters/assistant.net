using System;
using System.Linq;
using System.Threading;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
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
            services.TryAddSingleton<CommandClient>();
            services.TryAddSingleton<ICommandClient>(provider => ActivatorUtilities.CreateInstance<CommandClientInterceptableProxy>(
                provider,
                provider.GetRequiredService<CommandClient>()));
            return services;
        }

        public static IServiceCollection AddCommandOptions(this IServiceCollection services, Action<CommandConfigurationBuilder> configure)
        {
            var builder = Build(configure);

            foreach (var (_, implementation) in builder.Interceptors)
                services.TryAddTransient(implementation);

            foreach (var (_, implementation) in builder.Handlers)
                services.TryAddTransient(implementation);

            return services.Configure<CommandOptions>(opt =>
            {
                opt.Interceptors = builder.Interceptors.AsReadOnly();
                opt.Handlers = builder.Handlers.AsReadOnly();
            });
        }

        private static CommandConfigurationBuilder Build(Action<CommandConfigurationBuilder> configure)
        {
            var builder = new CommandConfigurationBuilder();
            configure?.Invoke(builder);

            var messages = builder.Handlers
                   .GroupBy(x => x.Key, x => x.Value)
                   .Where(x => x.Count() > 1)
                   .Select(FormatMessage);
            if (messages.Any())
            {
                var aggregatedMessage = string.Join(Environment.NewLine, messages);
                throw new InvalidOperationException(aggregatedMessage);
            }

            return builder;
        }

        private static string FormatMessage(IGrouping<Type, Type> duplicated)
        {
            var commandType = duplicated.Key;
            var implementationTypes = string.Join(" and ", duplicated);
            return $"{commandType} has multiple handlers {implementationTypes}.";
        }
    }
}