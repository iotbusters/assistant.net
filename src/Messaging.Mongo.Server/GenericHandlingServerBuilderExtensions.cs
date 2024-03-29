﻿using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB remote message handling configuration extensions for a server.
/// </summary>
public static class GenericHandlingServerBuilderExtensions
{
    /// <summary>
    ///     Configures server message handling to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static GenericHandlingServerBuilder UseMongo(this GenericHandlingServerBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .ConfigureStorage(builder.Name, b => b.UseMongo(configureOptions))
            .AddHealthChecks()
            .AddMongo(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures server message handling to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static GenericHandlingServerBuilder UseMongo(this GenericHandlingServerBuilder builder, string connectionString) => builder
        .UseMongo(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures server message handling to use MongoDB provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static GenericHandlingServerBuilder UseMongo(this GenericHandlingServerBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .ConfigureStorage(builder.Name, b => b.UseMongo(configuration))
            .AddHealthChecks()
            .AddMongo(builder.Name);
        return builder;
    }
}
