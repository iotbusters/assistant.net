﻿using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based messaging client configuration extensions.
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
                .ConfigureMongoOptions(builder.Name, configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(builder.Name, configuration);
            return builder;
        }
    }
}
