using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net;

/// <summary>
///     Builder marker extensions for SQLite provider.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static TBuilder UseSqlite<TBuilder>(this IBuilder<TBuilder> builder, string connectionString) => builder
        .UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static TBuilder UseSqlite<TBuilder>(this IBuilder<TBuilder> builder, SqliteConnection connection)
    {
        builder.Services.ConfigureSqliteOptions(builder.Name, o => o.Connection(connection));
        return builder.Instance;
    }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static TBuilder UseSqlite<TBuilder>(this IBuilder<TBuilder> builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services.ConfigureSqliteOptions(builder.Name, configureOptions);
        return builder.Instance;
    }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static TBuilder UseSqlite<TBuilder>(this IBuilder<TBuilder> builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureSqliteOptions(builder.Name, configuration);
        return builder.Instance;
    }
}
