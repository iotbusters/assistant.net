using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     SQLite based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures SQLite based generic message handling.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures SQLite based generic message handling.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseSqlite(configureOptions));
        return builder;
    }

    /// <summary>
    ///     Configures SQLite based generic message handling.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseSqlite(configuration));
        return builder;
    }
}
