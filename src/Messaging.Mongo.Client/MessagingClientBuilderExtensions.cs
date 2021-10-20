﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based remote messaging client configuration extensions for a client.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Configures message handling to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder ConfigureMongoHandlingClient(this MessagingClientBuilder builder, Action<MongoHandlingClientOptions> configureOptions)
        {
            builder.Services.ConfigureMongoHandlingClientOptions(configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures message handling to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder ConfigureMongoHandlingClient(this MessagingClientBuilder builder, IConfigurationSection configuration)
        {
            builder.Services.ConfigureMongoHandlingClientOptions(configuration);
            return builder;
        }

        /// <summary>
        ///     Registers remote MongoDB based handler of <typeparamref name="TMessage" /> from a client.
        /// </summary>
        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
            where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

        /// <summary>
        ///     Registers remote MongoDB based handler of <paramref name="messageType" /> from a client.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MessagingClientBuilder AddMongo(this MessagingClientBuilder builder, Type messageType)
        {
            if (messageType.GetResponseType() == null)
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            var handlerAbstractionType = typeof(IMessageHandler<,>).MakeGenericTypeBoundToMessage(messageType);
            var handlerImplementationType = typeof(MongoMessageHandlerProxy<,>).MakeGenericTypeBoundToMessage(messageType);

            builder.Services
                .TryAddSingleton<ExceptionModelConverter>()
                .ConfigureSerializer(b => b.AddJsonType(messageType))
                .ReplaceTransient(handlerAbstractionType, handlerImplementationType);
            return builder;
        }
    }
}