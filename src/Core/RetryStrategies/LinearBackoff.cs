using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.RetryStrategies;

/// <summary>
///     Linear backoff implementation of retrying strategy.
/// </summary>
public sealed class LinearBackoff : IRetryStrategy
{
    /// <inheritdoc/>
    public int MaxAttemptNumber { get; set; }

    /// <summary>
    ///     Max allowed time to delay before next handling attempt. Ignored if null.
    /// </summary>
    [Time("00:00:00.001", "23:59:59")]
    public TimeSpan? MaxDelayTime { get; set; }

    /// <summary>
    ///     Base interval time.
    /// </summary>
    [Required, Time("00:00:00.001", "23:59:59")]
    public TimeSpan Interval { get; set; }

    /// <summary>
    ///     Linear rate describing how <see cref="Interval"/> changes per attempt.
    /// </summary>
    [Required, Range(0.001d, 1000d)]
    public double Rate { get; set; }

    /// <inheritdoc/>
    public TimeSpan DelayTime(int attemptNumber)
    {
        var backoff = Interval * Rate * attemptNumber;

        if (MaxDelayTime == null || MaxDelayTime.Value >= backoff)
            return backoff;

        return MaxDelayTime.Value;
    }
}
