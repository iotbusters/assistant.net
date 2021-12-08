using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based messaging client configuration extensions for a client.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Registers remote MongoDB based handler of <typeparamref name="TMessage" /> from a client.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires calling one of UseMongo method.
        /// </remarks>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

        /// <summary>
        ///     Registers remote MongoDB based handler of <paramref name="messageType" /> from a client.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires calling one of UseMongo method.
        /// </remarks>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongo(this MessagingClientBuilder builder, Type messageType)
        {
            if (!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            builder.Services
                .TryAddSingleton<ExceptionModelConverter>()
                .ConfigureMessagingClientOptions(builder.Name, o => o.AddMongo(messageType));
            return builder;
        }
    }
}
