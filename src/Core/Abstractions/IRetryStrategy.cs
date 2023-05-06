using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.Configuration;
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
    ///     Reads configured backoff strategy.
    /// </summary>
    /// <remarks>
    ///     For example: <c>{ type: Constant, maxAttemptNumber: 5, interval: 00:00:01 }</c>.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public static IRetryStrategy ReadStrategy(IConfigurationSection configuration) => configuration["type"] switch
    {
        "Exponential" => configuration.Get<ExponentialBackoff>()!,
        "Linear" => configuration.Get<LinearBackoff>()!,
        "Constant" => configuration.Get<ConstantBackoff>()!,
        _ => throw new ArgumentException($"Key 'type' at {configuration.Path} is expected to be: "
                                         + $"'Exponential', 'Linear' or 'Constant' but was '{configuration["type"]}'.")
    };

    /// <summary>
    ///     Total time of all delays.
    /// </summary>
    public TimeSpan TotalTime => Enumerable.Range(1, MaxAttemptNumber).Select(DelayTime).Aggregate((x, y) => x + y);

    /// <summary>
    ///     Max allowed handling attempts before failure.
    /// </summary>
    [Required, Range(minimum: 1, maximum: 1000)]
    public int MaxAttemptNumber { get; }

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
