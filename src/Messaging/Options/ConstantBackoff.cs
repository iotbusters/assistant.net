using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Constant backoff implementation of retrying strategy.
    /// </summary>
    public class ConstantBackoff : IRetryStrategy
    {
        /// <inheritdoc/>
        public int MaxAttemptNumber { get; set; } = 5;
        
        /// <summary>
        ///     Constant interval time.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        /// <inheritdoc/>
        public TimeSpan DelayTime(int attemptNumber) => Interval;
    }
}
