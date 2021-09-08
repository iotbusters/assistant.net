using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Serialization;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Messaging client configuration extensions.
    /// </summary>
    public static class MessagingClientOptionsExtensions
    {
        /// <summary>
        ///     Registers a local in-memory handler type <typeparamref name="THandler" />.
        /// </summary>
        public static MessagingClientBuilder AddLocal<THandler>(this MessagingClientBuilder builder)
            where THandler : class, IAbstractHandler
        {
            var abstractHandlerTypes = typeof(THandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMessageHandler<,>));
            foreach (var abstractHandlerType in abstractHandlerTypes)
                builder.Services.ReplaceTransient(abstractHandlerType, typeof(THandler));
            return builder;
        }

        /// <summary>
        ///     Registers remote handler of <typeparamref name="TMessage" />.
        ///     Pay attention, it requires <see cref="IRemoteMessagingClient" /> remote handling provider implementation.
        /// </summary>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddRemote<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddRemote(typeof(TMessage));

        /// <summary>
        ///     Registers remote handler of <paramref name="messageType" />.
        ///     Pay attention, it requires <see cref="IRemoteMessagingClient" /> remote handling provider implementation.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddRemote(this MessagingClientBuilder builder, Type messageType)
        {
            if (messageType.GetResponseType() == null)
                throw new ArgumentException("Invalid message type.", nameof(messageType));

            var handlerAbstractionType = typeof(IMessageHandler<,>).MakeGenericTypeBoundToMessage(messageType);
            var handlerImplementationType = typeof(RemoteMessageHandlerProxy<,>).MakeGenericTypeBoundToMessage(messageType);

            builder.Services
                .ReplaceTransient(handlerAbstractionType, handlerImplementationType)
                .ConfigureSerializer(b => b.AddJsonType(messageType));
            return builder;
        }

        /// <summary>
        ///     Apply a configuration type <typeparamref name="TConfiguration" />.
        /// </summary>
        public static MessagingClientBuilder AddConfiguration<TConfiguration>(this MessagingClientBuilder builder)
            where TConfiguration : IMessageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

        /// <summary>
        ///     Apply a list of configuration instances <paramref name="messageConfigurations" />.
        /// </summary>
        public static MessagingClientBuilder AddConfiguration(this MessagingClientBuilder builder, params IMessageConfiguration[] messageConfigurations)
        {
            foreach (var config in messageConfigurations)
                config.Configure(builder);
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> to the end of the list.
        /// </summary>
        public static MessagingClientBuilder AddInterceptor<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class, IAbstractInterceptor
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Add(typeof(TInterceptor)));
            builder.Services.TryAddTransient<TInterceptor>();
            return builder;
        }

        /// <summary>
        ///     Adds an interceptor type <typeparamref name="TInterceptor" /> at the beginning of the list.
        /// </summary>
        public static MessagingClientBuilder AddInterceptorOnTop<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class, IAbstractInterceptor
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Insert(0, typeof(TInterceptor)));
            return builder;
        }

        /// <summary>
        ///     Removes all interceptors from the list.
        /// </summary>
        public static MessagingClientBuilder ClearInterceptors(this MessagingClientBuilder builder)
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an interceptor type <typeparamref name="TInterceptor" /> from the list.
        /// </summary>
        public static MessagingClientBuilder Remove<TInterceptor>(this MessagingClientBuilder builder)
            where TInterceptor : class, IAbstractInterceptor
        {
            builder.Services.ConfigureMessagingClientOptions(o => o.Interceptors.Remove(typeof(TInterceptor)));
            return builder;
        }
    }
}