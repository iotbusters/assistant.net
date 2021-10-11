using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Options
{
    /// <summary>
    ///     MongoDB client configuration for specific storage usage.
    /// </summary>
    public class MongoOptions
    {
        /// <summary>
        ///     MongoDB server connection string.
        /// </summary>
        [Required, MinLength(10)]//10:  mongodb://
        public string ConnectionString { get; set; } = null!;
    }
}
