using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;

namespace Assistant.Net.Messaging
{
    public static class ConfigurationExtensions
    {
        public static CommandConfigurationBuilder Add<TConfiguration>(this CommandConfigurationBuilder builder)
            where TConfiguration : ICommandConfiguration, new()
        {
            new TConfiguration().Configure(builder);
            return builder;
        }

        public static List<HandlerDefinition> Add<THandler>(this List<HandlerDefinition> list)
            where THandler : class, ICommandHandler
        {
            list.Add(HandlerDefinition.Create<THandler>());
            return list;
        }

        public static List<HandlerDefinition> Insert<THandler>(this List<HandlerDefinition> list, int index)
            where THandler : class, ICommandHandler
        {
            list.Insert(index, HandlerDefinition.Create<THandler>());
            return list;
        }

        public static List<InterceptorDefinition> Add<TInterceptor>(this List<InterceptorDefinition> list)
            where TInterceptor : class, IInterceptor
        {
            list.Add(InterceptorDefinition.Create<TInterceptor>());
            return list;
        }

        public static List<InterceptorDefinition> InsertAt<TInterceptor>(this List<InterceptorDefinition> list, int index)
            where TInterceptor : class, IInterceptor
        {
            list.Insert(index, InterceptorDefinition.Create<TInterceptor>());
            return list;
        }
    }
}