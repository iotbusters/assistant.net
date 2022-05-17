using Assistant.Net.Options;

namespace Assistant.Net
{
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
    }
}
