using Assistant.Net.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging.Options;

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
        Services.ConfigureStorage(b => b.UseMongo(configureOptions));
        return this;
    }

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public MongoHandlingBuilder UseMongo(IConfigurationSection configuration)
    {
        Services.ConfigureStorage(b => b.UseMongo(configuration));
        return this;
    }
}
