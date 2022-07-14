using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Options;

/// <summary>
///     SQLite client configuration for specific provider usage.
/// </summary>
public sealed class SqliteOptions
{
    /// <summary>
    ///     Sqlite server connection string.
    /// </summary>
    [Required, MinLength(11)]//12:  Data Source=
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    ///     Single SQLite connection instance.
    /// </summary>
    /// <remarks>
    ///     if defined the <see cref="ConnectionString"/> will be ignored.
    /// </remarks>
    public SqliteConnection? Connection { get; set; } = null!;

    /// <summary>
    ///     Determine if database should be configured before use.
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; } = false;
}
