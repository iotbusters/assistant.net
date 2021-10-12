using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB client configuration used for remote message handling coordination.
    /// </summary>
    public class MongoOptions
    {
        /// <summary>
        ///     Server connection string.
        /// </summary>
        [Required, MinLength(10)]//10:  mongodb://
        public string ConnectionString { get; set; } = null!;
    }
}
