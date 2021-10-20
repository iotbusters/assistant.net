using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     WEB oriented remote messaging client configuration extensions for a server.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Registers WEB oriented <typeparamref name="TMessageHandler" /> type accepting remote message handling requests on a server.
        /// </summary>
        /// <typeparam name="TMessageHandler">Specific message handler type.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWebHandler<TMessageHandler>(this MessagingClientBuilder builder)
            where TMessageHandler : class, IAbstractHandler => builder.AddWebHandler(typeof(TMessageHandler));

        /// <summary>
        ///     Registers WEB oriented <paramref name="handlerType" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWebHandler(this MessagingClientBuilder builder, Type handlerType)
        {
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerType));

            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerType))
                .ConfigureWebHandlingServerOptions(o => o.MessageTypes.AddRange(messageTypes));

            return builder;
        }
    }
}
