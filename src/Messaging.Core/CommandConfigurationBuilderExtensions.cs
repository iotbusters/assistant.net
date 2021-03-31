using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;

namespace Assistant.Net.Messaging
{
    public static class CommandConfigurationBuilderExtensions
    {
        public static CommandConfigurationBuilder Add<TConfiguration>(this CommandConfigurationBuilder builder)
            where TConfiguration : ICommandConfiguration, new() => builder.Add(new TConfiguration());

        public static CommandConfigurationBuilder Add(this CommandConfigurationBuilder builder, ICommandConfiguration configuration)
        {
            configuration.Configure(builder);
            return builder;
        }

        public static List<HandlerDefinition> AddLocal<THandler>(this List<HandlerDefinition> list)
            where THandler : class, IAbstractCommandHandler
        {
            list.Add(HandlerDefinition.Create<THandler>());
            return list;
        }

        public static List<InterceptorDefinition> Add<TInterceptor>(this List<InterceptorDefinition> list)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Add(InterceptorDefinition.Create<TInterceptor>());
            return list;
        }

        public static List<InterceptorDefinition> InsertAt<TInterceptor>(this List<InterceptorDefinition> list, int index)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Insert(index, InterceptorDefinition.Create<TInterceptor>());
            return list;
        }
    }
}