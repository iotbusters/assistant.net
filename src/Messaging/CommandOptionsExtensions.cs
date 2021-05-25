using System.Linq;
using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    public static class CommandOptionsExtensions
    {
        /// <summary>
        ///     Registers a configuration type <typeparamref name="TConfiguration" />.
        /// </summary>
        public static CommandOptions Add<TConfiguration>(this CommandOptions options)
            where TConfiguration : ICommandConfiguration, new() => options.Add(new TConfiguration());

        /// <summary>
        ///     Registers a configuration instance <paramref name="commandConfigurations" />.
        /// </summary>
        public static CommandOptions Add(this CommandOptions options, params ICommandConfiguration[] commandConfigurations)
        {
            foreach (var config in commandConfigurations)
                config.Configure(options);
            return options;
        }

        /// <summary>
        ///     Registers a local in-memory handler type <typeparamref name="THandler" />.
        /// </summary>
        public static ISet<HandlerDefinition> AddLocal<THandler>(this ISet<HandlerDefinition> set)
            where THandler : class, IAbstractCommandHandler
        {
            set.Add(HandlerDefinition.Create<THandler>());
            return set;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> to the end of the list.
        /// </summary>
        public static IList<InterceptorDefinition> Add<TInterceptor>(this IList<InterceptorDefinition> list)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Add(InterceptorDefinition.Create<TInterceptor>());
            return list;
        }

        /// <summary>
        ///     Adds an interceptor type typeparamref name="TInterceptor" /> at specific possition in the list.
        /// </summary>
        public static IList<InterceptorDefinition> InsertAt<TInterceptor>(this IList<InterceptorDefinition> list, int index)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            list.Insert(index, InterceptorDefinition.Create<TInterceptor>());
            return list;
        }

        /// <summary>
        ///     Removes all interceptors from the list.
        /// </summary>
        public static IList<InterceptorDefinition> ClearAll(this IList<InterceptorDefinition> list)
        {
            list.Clear();
            return list;
        }

        /// <summary>
        ///     Removes an interceptor type <typeparamref name="TInterceptor" /> from the list.
        /// </summary>
        public static IList<InterceptorDefinition> Remove<TInterceptor>(this IList<InterceptorDefinition> list)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            foreach (var definition in list.Where(x => x.Type == typeof(TInterceptor)))
                list.Remove(definition);
            return list;
        }
    }
}