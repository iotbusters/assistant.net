using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Message handling retry strategy.
/// </summary>
public interface IRetryStrategy
{
    /// <summary>
    ///     Total time of all delays.
    /// </summary>
    public TimeSpan TotalTime => Enumerable.Range(1, MaxAttemptNumber).Select(DelayTime).Aggregate((x, y) => x + y);

    /// <summary>
    ///     Max allowed handling attempts before failure.
    /// </summary>
    [Required, Range(minimum: 1, maximum: 1000)]
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
