using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB based message handling configuration builder on a server.
    /// </summary>
    public class MongoHandlingBuilder : MessagingClientBuilder<MongoHandlingBuilder>
    {
        /// <summary/>
        public MongoHandlingBuilder(IServiceCollection services) : base(services, MongoOptionsNames.DefaultName) { }

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public MongoHandlingBuilder UseMongo(string connectionString) =>
            UseMongo(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public MongoHandlingBuilder UseMongo(Action<MongoOptions> configureOptions)
        {
            Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(Name, configureOptions);
            return this;
        }

        /// <summary>
        ///     Configures the messaging client to connect a MongoDB database from a client.
        /// </summary>
        public MongoHandlingBuilder UseMongo(IConfigurationSection configuration)
        {
            Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(Name, configuration);
            return this;
        }
    }
}
