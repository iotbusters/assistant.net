using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net;

/// <summary>
///     Builder marker extensions for MongoDB provider.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static TBuilder UseMongo<TBuilder>(this IBuilder<TBuilder> builder, string connectionString) => builder
        .UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static TBuilder UseMongo<TBuilder>(this IBuilder<TBuilder> builder, Action<MongoOptions> configureOptions)
    {
        builder.Services.ConfigureMongoOptions(builder.Name, configureOptions);
        return builder.Instance;
    }

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static TBuilder UseMongo<TBuilder>(this IBuilder<TBuilder> builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureMongoOptions(builder.Name, configuration);
        return builder.Instance;
    }
}
