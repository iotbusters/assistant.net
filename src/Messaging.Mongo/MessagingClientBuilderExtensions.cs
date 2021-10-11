using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based remote messaging client configuration extensions.
    /// </summary>
    public static class MessagingClientBuilderExtensions
    {
        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) =>
            builder.UseMongo(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
        {
            builder.Services
                .AddMongoClient()
                .ConfigureMongoOptions(configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddMongoClient()
                .ConfigureMongoOptions(configuration);
            return builder;
        }
    }
}
