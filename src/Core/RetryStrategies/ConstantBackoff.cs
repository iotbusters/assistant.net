using Assistant.Net.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.RetryStrategies;

/// <summary>
///     Constant backoff implementation of retrying strategy.
/// </summary>
public class ConstantBackoff : IRetryStrategy
{
    /// <inheritdoc/>
    public int MaxAttemptNumber { get; set; }

    /// <summary>
    ///     Constant interval time.
    /// </summary>
    [Required]
    public TimeSpan Interval { get; set; }

    /// <inheritdoc/>
    public TimeSpan DelayTime(int attemptNumber) => Interval;
}
