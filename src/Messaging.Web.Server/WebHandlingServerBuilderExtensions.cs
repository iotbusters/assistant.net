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

        /// <summary>
        ///     Registers WEB oriented <paramref name="handlerInstance" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static WebHandlingServerBuilder AddHandler(this WebHandlingServerBuilder builder, object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerInstance))
                .ConfigureWebHandlingServerOptions(o => o.MessageTypes.AddRange(messageTypes));
            return builder;
        }

        /// <summary>
        ///     Removes all handlers from the list.
        /// </summary>
        public static WebHandlingServerBuilder ClearHandlers(this WebHandlingServerBuilder builder)
        {
            builder.Services
                .ConfigureMessagingClient(b => b.ClearHandlers())
                .ConfigureWebHandlingServerOptions(o => o.MessageTypes.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an handler of <typeparamref name="TMessage" /> from the list.
        /// </summary>
        public static WebHandlingServerBuilder Remove<TMessage>(this WebHandlingServerBuilder builder)
            where TMessage : class => builder.Remove(typeof(TMessage));

        /// <summary>
        ///     Removes an handler of <paramref name="messageType" /> from the list.
        /// </summary>
        public static WebHandlingServerBuilder Remove(this WebHandlingServerBuilder builder, Type messageType)
        {
            if (!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            builder.Services
                .ConfigureMessagingClient(b => b.Remove(messageType))
                .ConfigureWebHandlingServerOptions(o => o.MessageTypes.Remove(messageType));
            return builder;
        }

        /// <summary>
        ///     Removes the handler type <typeparamref name="THandler"/> from the list.
        /// </summary>
        public static WebHandlingServerBuilder RemoveHandler<THandler>(this WebHandlingServerBuilder builder)
            where THandler : class => builder.RemoveHandler(typeof(THandler));

        /// <summary>
        ///     Removes the <paramref name="handlerType" /> from the list.
        /// </summary>
        public static WebHandlingServerBuilder RemoveHandler(this WebHandlingServerBuilder builder, Type handlerType)
        {
            var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
            if (!handlerInterfaceTypes.Any())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services
                .ConfigureMessagingClient(b => b.RemoveHandler(handlerType))
                .ConfigureWebHandlingServerOptions(o =>
                {
                    var messageTypes = handlerInterfaceTypes.Select(x => x.GetGenericArguments().First());
                    foreach (var messageType in messageTypes)
                        o.MessageTypes.Remove(messageType);
                });
            return builder;
        }
    }
}
