using Assistant.Net.Messaging.Options;
using System;

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

            builder.Services.ConfigureMessagingClientOptions(WebOptionsNames.DefaultName, o => o.AddLocalHandler(handlerType));
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

            builder.Services.ConfigureMessagingClientOptions(WebOptionsNames.DefaultName, o => o.AddLocalHandler(handlerInstance));
            return builder;
        }

        /// <summary>
        ///     Removes all WEB oriented message handlers.
        /// </summary>
        public static WebHandlingServerBuilder ClearHandlers(this WebHandlingServerBuilder builder)
        {
            builder.Services.ConfigureMessagingClient(WebOptionsNames.DefaultName, b => b.ClearHandlers());
            return builder;
        }
    }
}
