using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     WEB oriented messaging client configuration extensions.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Configures the messaging client to connect the remote web handler.
        /// </summary>
        public static MessagingClientBuilder UseWeb(this MessagingClientBuilder builder, Action<IHttpClientBuilder> configureBuilder)
        {
            var clientBuilder = builder.Services.AddRemoteWebMessagingClient();
            configureBuilder.Invoke(clientBuilder);
            return builder;
        }

        /// <summary>
        ///     Registers remote WEB handler of <typeparamref name="TMessage" />.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires <see cref="IWebMessageHandlerClient" /> remote handling provider implementation.
        /// </remarks>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWeb<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddWeb(typeof(TMessage));

        /// <summary>
        ///     Registers remote WEB handler of <paramref name="messageType" />.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires <see cref="IWebMessageHandlerClient" /> remote handling provider implementation.
        /// </remarks>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddWeb(this MessagingClientBuilder builder, Type messageType)
        {
            if (messageType.GetResponseType() == null)
                throw new ArgumentException("Invalid message type.", nameof(messageType));

            var providerType = typeof(WebMessageHandlerProxy<,>);

            builder.Services.ConfigureMessagingClientOptions(o =>
            {
                o.Handlers.Remove(messageType);
                o.Handlers.Add(messageType, p =>
                {
                    var provider = ActivatorUtilities.CreateInstance(p, providerType.MakeGenericTypeBoundToMessage(messageType));
                    return (IAbstractHandler)provider;
                });
            });

            return builder;
        }
    }
}
