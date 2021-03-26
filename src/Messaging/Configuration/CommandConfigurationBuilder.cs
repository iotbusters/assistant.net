using System;
using System.Collections.Generic;
using System.Linq;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandConfigurationBuilder
    {
        internal List<KeyValuePair<Type,Type>> Interceptors { get; } = new();
        internal List<KeyValuePair<Type,Type>> Handlers { get; } = new();

        public CommandConfigurationBuilder AddInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor
        {
            var implementationType = typeof(TInterceptor);

            var interceptorTypes = implementationType.GetInterfaces().Where(x => x.GetGenericTypeDefinition() == typeof(ICommandInterceptor<>));
            foreach (var interceptorType in interceptorTypes)
                Interceptors.Add(new KeyValuePair<Type,Type>(interceptorType, implementationType));

            return this;
        }

        public CommandConfigurationBuilder AddHandler<THandler>() where THandler : class, ICommandHandler
        {
            var implementationType = typeof(THandler);

            var handlerTypes = implementationType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                .Select(x => x.GetGenericArguments().First());
            foreach (var commandType in handlerTypes)
                Handlers.Add(new KeyValuePair<Type,Type>(commandType, implementationType));

            return this;
        }

        public CommandConfigurationBuilder Add<TConfiguration>() where TConfiguration : ICommandHandlerConfiguration, new()
        {
            new TConfiguration().Configure(this);
            return this;
        }
    }
}