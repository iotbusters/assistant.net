using Assistant.Net.Options;
using Microsoft.Data.Sqlite;

namespace Assistant.Net;

/// <summary>
///     SQLite options extensions.
/// </summary>
public static class SqliteOptionsExtensions
{
    /// <summary>
    ///     Configures SQLite database connection string.
    /// </summary>
    public static SqliteOptions Connection(this SqliteOptions options, string connectionString)
    {
        options.ConnectionString = connectionString;
        return options;
    }
    /// <summary>
    ///     Configures SQLite database connection instance.
    /// </summary>
    public static SqliteOptions Connection(this SqliteOptions options, SqliteConnection connection)
    {
        options.Connection = connection;
        options.ConnectionString = connection.ConnectionString;
        return options;
    }

    /// <summary>
    ///     Configures SQLite database initialization.
    /// </summary>
    public static SqliteOptions EnsureCreated(this SqliteOptions options, bool ensureDatabaseCreated = true)
    {
        options.EnsureDatabaseCreated = ensureDatabaseCreated;
        return options;
    }
}
