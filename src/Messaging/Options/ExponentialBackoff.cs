using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Exponential backoff implementation of retrying strategy.
    /// </summary>
    public class ExponentialBackoff : IRetryStrategy
    {
        /// <inheritdoc/>
        public int MaxAttemptNumber { get; set; } = 5;

        /// <summary>
        ///     Max allowed time to delay before next handling attempt. Ignored if null.
        /// </summary>
        public TimeSpan? MaxDelayTime { get; set; }

        /// <summary>
        ///     Base interval time.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        ///     Exponential rate describing how <see cref="Interval"/> changes per attempt.
        /// </summary>
        public double Rate { get; set; } = 1.2;

        /// <inheritdoc/>
        public TimeSpan DelayTime(int attemptNumber)
        {
            var backoff = Interval * Math.Pow(Rate, attemptNumber);

            if (MaxDelayTime == null || MaxDelayTime.Value >= backoff)
                return backoff;

            return MaxDelayTime.Value;
        }
    }
}
