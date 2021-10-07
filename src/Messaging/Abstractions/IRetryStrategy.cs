using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message handling retry strategy.
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        ///     Max allowed handling attempts before failure.
        /// </summary>
        [Range(minimum: 1, maximum: 500)]
        public int MaxAttemptNumber { get; set; }

        /// <summary>
        ///     Determines if the attempt <paramref name="attemptNumber"/> can be retried.
        /// </summary>
        public virtual bool CanRetry(int attemptNumber) =>
            attemptNumber <= MaxAttemptNumber;

        /// <summary>
        ///     Time to delay before next handling attempt.
        /// </summary>
        TimeSpan DelayTime(int attemptNumber);
    }
}
