using System;
using System.Linq;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    public static class CommandClientOptionsExtensions
    {
        /// <summary>
        ///     Registers a local in-memory handler type <typeparamref name="THandler" />.
        /// </summary>
        public static CommandClientBuilder AddLocal<THandler>(this CommandClientBuilder builder)
            where THandler : class, IAbstractCommandHandler
        {
            var abstractHandlerTypes = typeof(THandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
            foreach (var abstractHandlerType in abstractHandlerTypes)
                builder.Services.ReplaceTransient(abstractHandlerType, typeof(THandler));
            return builder;
        }

        /// <summary>
        ///     Registers remote handler of <typeparamref name="TCommand" />.
        ///     Pay attention, it requires <see cref="IRemoteCommandClient" /> remote handling provider implementation.
        /// </summary>
        /// <param name="set">Set of existing command handler definitions.</param>
        /// <typeparam name="TCommand">Specific command type to be handled remotely.</typeparam>
        public static CommandClientBuilder AddRemote<TCommand>(this CommandClientBuilder builder)
            where TCommand : class, IAbstractCommand => builder.AddRemote(typeof(TCommand));

        /// <summary>
        ///     Registers remote handler of <paramref name="commandType" />.
        ///     Pay attention, it requires <see cref="IRemoteCommandClient" /> remote handling provider implementation.
        /// </summary>
        /// <param name="set">Set of existing command handler definitions.</param>
        /// <param name="commandType">Specific command type to be handled remotely.</param>
        public static CommandClientBuilder AddRemote(this CommandClientBuilder builder, Type commandType)
        {
            if (commandType.GetResponseType() == null)
                throw new ArgumentException("Invalid command type.", nameof(commandType));

            var handlerAbstractionType = typeof(ICommandHandler<,>).MakeGenericTypeBoundToCommand(commandType);
            var handlerImplementationType = typeof(RemoteCommandHandlerProxy<,>).MakeGenericTypeBoundToCommand(commandType);

            builder.Services.ReplaceTransient(handlerAbstractionType, handlerImplementationType);
            return builder;
        }

        /// <summary>
        ///     Apply a configuration type <typeparamref name="TConfiguration" />.
        /// </summary>
        public static CommandClientBuilder AddConfiguration<TConfiguration>(this CommandClientBuilder builder)
            where TConfiguration : ICommandConfiguration, new() => builder.AddConfiguration(new TConfiguration());

        /// <summary>
        ///     Apply a list of configuration instances <paramref name="commandConfigurations" />.
        /// </summary>
        public static CommandClientBuilder AddConfiguration(this CommandClientBuilder builder, params ICommandConfiguration[] commandConfigurations)
        {
            foreach (var config in commandConfigurations)
                config.Configure(builder);
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> to the end of the list.
        /// </summary>
        public static CommandClientBuilder AddInterceptor<TInterceptor>(this CommandClientBuilder builder)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            builder.Services.ConfigureCommandClientOptions(o => o.Interceptors.Add(typeof(TInterceptor)));
            builder.Services.ReplaceTransient<TInterceptor>();
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> at the beginning of the list.
        /// </summary>
        public static CommandClientBuilder AddInterceptorOnTop<TInterceptor>(this CommandClientBuilder builder)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            builder.AddInterceptorOnTop<TInterceptor>();
            return builder;
        }

        /// <summary>
        ///     Removes all interceptors from the list.
        /// </summary>
        public static CommandClientBuilder ClearInterceptors(this CommandClientBuilder builder)
        {
            builder.Services.ConfigureCommandClientOptions(o => o.Interceptors.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an interceptor type <typeparamref name="TInterceptor" /> from the list.
        /// </summary>
        public static CommandClientBuilder Remove<TInterceptor>(this CommandClientBuilder builder)
            where TInterceptor : class, IAbstractCommandInterceptor
        {
            builder.Services.ConfigureCommandClientOptions(o => o.Interceptors.Remove(typeof(TInterceptor)));
            return builder;
        }
    }
}