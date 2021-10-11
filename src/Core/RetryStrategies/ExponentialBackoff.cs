﻿using Assistant.Net.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.RetryStrategies
{
    /// <summary>
    ///     Exponential backoff implementation of retrying strategy.
    /// </summary>
    public class ExponentialBackoff : IRetryStrategy
    {
        /// <inheritdoc/>
        public int? MaxAttemptNumber { get; set; }

        /// <summary>
        ///     Max allowed time to delay before next handling attempt. Ignored if null.
        /// </summary>
        public TimeSpan? MaxDelayTime { get; set; }

        /// <summary>
        ///     Base interval time.
        /// </summary>
        [Required]
        public TimeSpan? Interval { get; set; }

        /// <summary>
        ///     Exponential rate describing how <see cref="Interval"/> changes per attempt.
        /// </summary>
        [Required]
        public double? Rate { get; set; }

        /// <inheritdoc/>
        public TimeSpan DelayTime(int attemptNumber)
        {
            var backoff = Interval!.Value * Math.Pow(Rate!.Value, attemptNumber);

            if (MaxDelayTime == null || MaxDelayTime.Value >= backoff)
                return backoff;

            return MaxDelayTime.Value;
        }
    }
}