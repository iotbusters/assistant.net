using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB client configuration used for remote message handling coordination.
    /// </summary>
    public class MongoHandlingClientOptions
    {
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
