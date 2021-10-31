using Assistant.Net.Messaging.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     WEB oriented messaging handling configuration extensions for a server.
    /// </summary>
    public static class WebHandlingServerBuilderExtensions
    {
        /// <summary>
        ///     Registers WEB oriented <typeparamref name="TMessageHandler" /> type accepting remote message handling requests on a server.
        /// </summary>
        /// <typeparam name="TMessageHandler">Specific message handler type.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static WebHandlingServerBuilder AddHandler<TMessageHandler>(this WebHandlingServerBuilder builder)
            where TMessageHandler : class => builder.AddHandler(typeof(TMessageHandler));

        /// <summary>
        ///     Registers WEB oriented <paramref name="handlerType" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static WebHandlingServerBuilder AddHandler(this WebHandlingServerBuilder builder, Type handlerType)
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
