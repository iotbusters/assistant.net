using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.RetryStrategies;

/// <summary>
///     Exponential backoff implementation of retrying strategy.
/// </summary>
public sealed class ExponentialBackoff : IRetryStrategy
{
    /// <inheritdoc/>
    public int MaxAttemptNumber { get; set; }

    /// <summary>
    ///     Max allowed time to delay before next handling attempt. Ignored if null.
    /// </summary>
    [Time("00:00:00.001", "1.00:00:00")]
    public TimeSpan? MaxDelayTime { get; set; }

    /// <summary>
    ///     Base interval time.
    /// </summary>
    [Required, Time("00:00:00.001", "1.00:00:00")]
    public TimeSpan Interval { get; set; }

    /// <summary>
    ///     Exponential rate describing how <see cref="Interval"/> changes per attempt.
    /// </summary>
    [Required, Range(0.01d, 100d)]
    public double Rate { get; set; }

    /// <inheritdoc/>
    public TimeSpan DelayTime(int attemptNumber)
    {
        var backoff = Interval * Math.Pow(Rate, attemptNumber);

        if (MaxDelayTime == null || MaxDelayTime.Value >= backoff)
            return backoff;

        return MaxDelayTime.Value;
    }
}
