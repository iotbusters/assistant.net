﻿//using Assistant.Net.Messaging.Abstractions;
//using Assistant.Net.Messaging.Options;
//using Assistant.Net.Messaging.Serialization;
//using Microsoft.Extensions.Configuration;
//using System;

//namespace Assistant.Net.Messaging
//{
//    /// <summary>
//    ///     SQLite based messaging client configuration extensions for a client.
//    /// </summary>
//    public static class MessagingClientBuilderExtensions
//    {
//        /// <summary>
//        ///     Configures the messaging client to connect a SQLite database from a client.
//        /// </summary>
//        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) =>
//            builder.UseMongo(o => o.ConnectionString = connectionString);

//        /// <summary>
//        ///     Configures the messaging client to connect a SQLite database from a client.
//        /// </summary>
//        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<SqliteOptions> configureOptions)
//        {
//            builder.Services
//                .AddMongoClientFactory()
//                .ConfigureSqliteOptions(builder.Name, configureOptions);
//            return builder;
//        }

//        /// <summary>
//        ///     Configures the messaging client to connect a MongoDB database from a client.
//        /// </summary>
//        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
//        {
//            builder.Services
//                .AddMongoClientFactory()
//                .ConfigureSqliteOptions(builder.Name, configuration);
//            return builder;
//        }

//        /// <summary>
//        ///     Registers remote SQLite based handler of <typeparamref name="TMessage" /> from a client.
//        /// </summary>
//        /// <remarks>
//        ///     Pay attention, it requires calling one of UseMongo method.
//        /// </remarks>
//        /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
//        /// <exception cref="ArgumentException"/>
//        public static MessagingClientBuilder AddSqlite<TMessage>(this MessagingClientBuilder builder)
//            where TMessage : class, IAbstractMessage => builder.AddSqlite(typeof(TMessage));

//        /// <summary>
//        ///     Registers remote SQLite based handler of <paramref name="messageType" /> from a client.
//        /// </summary>
//        /// <remarks>
//        ///     Pay attention, it requires calling one of UseMongo method.
//        /// </remarks>
//        /// <exception cref="ArgumentException"/>
//        public static MessagingClientBuilder AddSqlite(this MessagingClientBuilder builder, Type messageType)
//        {
//            if (!messageType.IsMessage())
//                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

//            builder.Services
//                .TryAddSingleton<ExceptionModelConverter>()
//                .ConfigureMessagingClientOptions(builder.Name, o => o.AddSqlite(messageType));
//            return builder;
//        }
//    }
//}
