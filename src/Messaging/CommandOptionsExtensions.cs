using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    public static class CommandOptionsExtensions
    {
        public static CommandOptions Add<TConfiguration>(this CommandOptions options)
            where TConfiguration : ICommandConfiguration, new() => options.Add(new TConfiguration());

        public static CommandOptions Add(this CommandOptions options, params ICommandConfiguration[] commandConfigurations)
        {
            foreach (var config in commandConfigurations)
                config.Configure(options);
            return options;
        }

        public static ISet<HandlerDefinition> AddLocal<THandler>(this ISet<HandlerDefinition> set)
            where THandler : class, IAbstractCommandHandler
        {
            set.Add(HandlerDefinition.Create<THandler>());
            return set;
        }

        public static IList<InterceptorDefinition> Add<TInterceptor>(this IList<InterceptorDefinition> list)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Add(InterceptorDefinition.Create<TInterceptor>());
            return list;
        }

        public static IList<InterceptorDefinition> InsertAt<TInterceptor>(this IList<InterceptorDefinition> list, int index)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Insert(index, InterceptorDefinition.Create<TInterceptor>());
            return list;
        }
    }
}