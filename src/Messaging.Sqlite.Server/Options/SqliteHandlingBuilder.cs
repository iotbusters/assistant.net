using Assistant.Net.Options;
using Assistant.Net.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     SQLite based message handling configuration builder on a server.
/// </summary>
public class SqliteHandlingBuilder : MessagingClientBuilder<SqliteHandlingBuilder>
{
    /// <summary/>
    public SqliteHandlingBuilder(IServiceCollection services) : base(services, SqliteOptionsNames.DefaultName) { }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public SqliteHandlingBuilder UseSqlite(string connectionString) =>
        UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public SqliteHandlingBuilder UseSqlite(SqliteConnection connection)
    {
        Services.ConfigureStorage(b => b.UseSqlite(connection));
        return this;
    }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public SqliteHandlingBuilder UseSqlite(Action<SqliteOptions> configureOptions)
    {
        Services.ConfigureStorage(b => b.UseSqlite(configureOptions));
        return this;
    }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public SqliteHandlingBuilder UseSqlite(IConfigurationSection configuration)
    {
        Services.ConfigureStorage(b => b.UseSqlite(configuration));
        return this;
    }
}
