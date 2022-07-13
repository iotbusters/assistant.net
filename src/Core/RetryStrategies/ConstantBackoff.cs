using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.RetryStrategies;

/// <summary>
///     Constant backoff implementation of retrying strategy.
/// </summary>
public sealed class ConstantBackoff : IRetryStrategy
{
    /// <inheritdoc/>
    public int MaxAttemptNumber { get; set; }

    /// <summary>
    ///     Constant interval time.
    /// </summary>
    [Required, Time("00:00:00.0000001", "1.00:00:00")]
    public TimeSpan Interval { get; set; }

    /// <inheritdoc/>
    public TimeSpan DelayTime(int attemptNumber) => Interval;
}
