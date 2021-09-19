using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Messaging client configuration extensions.
    /// </summary>
    public static class MessageClientBuilderExtensions
    {
        /// <summary>
        ///     Configures the messaging client to connect the remote web handler.
        /// </summary>
        public static MessagingClientBuilder UseWebHandler(this MessagingClientBuilder builder, Action<IHttpClientBuilder> configureBuilder)
        {
            var clientBuilder = builder.Services.AddRemoteWebMessagingClient();
            configureBuilder.Invoke(clientBuilder);
            return builder;
        }

        /// <summary>
        ///     Registers remote handler of <typeparamref name="TMessage" />.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires <see cref="IWebMessageHandlerClient" /> remote handling provider implementation.
        /// </remarks>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWeb<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddWeb(typeof(TMessage));

        /// <summary>
        ///     Registers remote handler of <paramref name="messageType" />.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires <see cref="IWebMessageHandlerClient" /> remote handling provider implementation.
        /// </remarks>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWeb(this MessagingClientBuilder builder, Type messageType)
        {
            if (messageType.GetResponseType() == null)
                throw new ArgumentException("Invalid message type.", nameof(messageType));

            var handlerAbstractionType = typeof(IMessageHandler<,>).MakeGenericTypeBoundToMessage(messageType);
            var handlerImplementationType = typeof(WebMessageHandlerProxy<,>).MakeGenericTypeBoundToMessage(messageType);

            builder.Services
                .ReplaceTransient(handlerAbstractionType, handlerImplementationType)
                .ConfigureSerializer(b => b.AddJsonType(messageType));
            return builder;
        }
    }
}
