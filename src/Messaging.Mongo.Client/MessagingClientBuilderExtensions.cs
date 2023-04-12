using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures MongoDB based generic message handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) => builder
        .UseMongo(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures MongoDB based generic message handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseMongo(configureOptions));
        return builder;
    }

    /// <summary>
    ///     Configures MongoDB based generic message handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseMongo(configuration));
        return builder;
    }
}
