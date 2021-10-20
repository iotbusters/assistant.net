using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based remote messaging client configuration extensions for a server.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Registers MongoDB based <typeparamref name="TMessageHandler" /> type accepting remote message handling requests on a server.
        /// </summary>
        /// <typeparam name="TMessageHandler">Specific message handler type.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongoHandler<TMessageHandler>(this MessagingClientBuilder builder)
            where TMessageHandler : class, IAbstractHandler => builder.AddMongoHandler(typeof(TMessageHandler));

        /// <summary>
        ///     Registers MongoDB based <paramref name="handlerType" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongoHandler(this MessagingClientBuilder builder, Type handlerType)
        {
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerType))
                .ConfigureMongoHandlingServerOptions(o =>
                {
                    var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
                    foreach (var messageType in messageTypes)
                        o.MessageTypes.Add(messageType);
                });

            return builder;
        }

        /// <summary>
        ///     Registers MongoDB based <paramref name="handlerInstance" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongoHandler(this MessagingClientBuilder builder, IAbstractHandler handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));
            
            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerInstance))
                .ConfigureMongoHandlingServerOptions(o =>
                {
                    var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
                    foreach (var messageType in messageTypes)
                        o.MessageTypes.Add(messageType);
                });

            return builder;
        }

    }
}
