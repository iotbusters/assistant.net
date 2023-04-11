using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     SQLite remote message handling configuration extensions for a server.
/// </summary>
public static class GenericHandlingServerBuilderExtensions
{
    /// <summary>
    ///     Configures SQLite provider for storage based messaging handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static GenericHandlingServerBuilder UseSqlite(this GenericHandlingServerBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services.ConfigureStorage(GenericOptionsNames.DefaultName, b => b
            .UseSqlite(configureOptions)
            .UseSqlite())
            .AddHealthChecks()
            .ReplaceSqlite(HealthCheckNames.SingleName);
        return builder;
    }

    /// <summary>
    ///     Configures SQLite provider for storage based messaging handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static GenericHandlingServerBuilder UseSqlite(this GenericHandlingServerBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures SQLite provider for storage based messaging handling.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static GenericHandlingServerBuilder UseSqlite(this GenericHandlingServerBuilder builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureStorage(GenericOptionsNames.DefaultName, b => b
            .UseSqlite(configuration)
            .UseSqlite())
            .AddHealthChecks()
            .ReplaceSqlite(HealthCheckNames.SingleName);
        return builder;
    }
}
