using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based messaging client configuration extensions for a client.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) =>
            builder.UseMongo(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(MongoOptions.ClientName, configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(MongoOptions.ClientName, configuration);
            return builder;
        }

        /// <summary>
        ///     Registers remote MongoDB based handler of <typeparamref name="TMessage" /> from a client.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires calling one of <see cref="UseMongo(MessagingClientBuilder,string)"/>.
        /// </remarks>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

        /// <summary>
        ///     Registers remote MongoDB based handler of <paramref name="messageType" /> from a client.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it requires calling one of <see cref="UseMongo(MessagingClientBuilder,string)"/>.
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
