using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     MongoDB client configurations used for remote message handling coordination.
    /// </summary>
    public class MongoHandlingServerOptions
    {
        /// <summary>
        ///     MongoDB database name.
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = MongoNames.DatabaseName;

        /// <summary>
        ///     Time to delay after no messages to handle were found.
        /// </summary>
        public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     Time to delay before next message handling attempt.
        /// </summary>
        public TimeSpan NextMessageDelayTime { get; set; } = TimeSpan.FromSeconds(0.01);
    }
}
