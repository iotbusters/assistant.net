using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Storage.Mongo.Options
{
    /// <summary>
    ///     MongoDB client configurations used for specific storage usage.
    /// </summary>
    public class MongoOptions
    {
        /// <summary>
        ///     MongoDB server connection string.
        /// </summary>
        [Required, MinLength(10)]// mongodb://
        public string ConnectionString { get; set; } = null!;
    }
}
