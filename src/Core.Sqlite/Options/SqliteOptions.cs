using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Options;

/// <summary>
///     SQLite client configuration for specific provider usage.
/// </summary>
public class SqliteOptions
{
    /// <summary>
    ///     Sqlite server connection string.
    /// </summary>
    [Required, MinLength(11)]//12:  Data Source=
    public string ConnectionString { get; set; } = null!;
}
