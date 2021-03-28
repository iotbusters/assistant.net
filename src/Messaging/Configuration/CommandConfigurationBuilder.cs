using System;
using System.Collections.Generic;
using System.Linq;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public sealed class CommandConfigurationBuilder
    {
        internal List<Type> Interceptors { get; } = new();
        internal List<Type> Handlers { get; } = new();

        public CommandConfigurationBuilder AddInterceptor<TInterceptor>() where TInterceptor : class, IInterceptor
        {
            Interceptors.Add(typeof(TInterceptor));
            return this;
        }

        public CommandConfigurationBuilder AddHandler<THandler>() where THandler : class, ICommandHandler
        {
            Handlers.Add(typeof(THandler));
            return this;
        }

        public CommandConfigurationBuilder Add<TConfiguration>() where TConfiguration : ICommandHandlerConfiguration, new()
        {
            new TConfiguration().Configure(this);
            return this;
        }
    }
}