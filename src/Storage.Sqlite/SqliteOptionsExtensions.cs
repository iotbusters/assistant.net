using Assistant.Net.Storage.Options;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Options extensions for SQLite based remote message handling.
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
