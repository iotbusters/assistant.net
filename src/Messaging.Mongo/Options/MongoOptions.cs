using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;
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

        /// <summary>
        ///     Database name.
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = MongoNames.DatabaseName;

        /// <summary>
        ///     Message handling response polling strategy.
        /// </summary>
        public IRetryStrategy ResponsePoll { get; set; } = new ConstantBackoff {MaxAttemptNumber = 4, Interval = TimeSpan.FromSeconds(0.1)};
    }
}
