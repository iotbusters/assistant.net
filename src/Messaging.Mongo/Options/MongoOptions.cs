using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB client configuration used for remote message handling coordination.
    /// </summary>
    public class MongoOptions
    {
        /// <summary>
        ///     MongoDB option name for a client.
        /// </summary>
        public const string ClientName = "mongo.client";

        /// <summary>
        ///     MongoDB option name for a server.
        /// </summary>
        public const string ServerName = "mongo.server";

        /// <summary>
        ///     Server connection string.
        /// </summary>
        [Required, MinLength(10)]//10:  mongodb://
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        ///     Database name.
        /// </summary>
        [Required, MinLength(1)]
        public string DatabaseName { get; set; } = MongoNames.DatabaseName;

    }
}
