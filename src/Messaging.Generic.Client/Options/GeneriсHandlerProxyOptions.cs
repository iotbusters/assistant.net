using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Storage based message handling client configuration.
/// </summary>
public sealed class GenericHandlerProxyOptions
{
    /// <summary>
    ///     Server instance ID.
    /// </summary>
    public int InstanceId { get; set; } = 1;

    /// <summary>
    ///     Message handling response polling strategy.
    /// </summary>
    public IRetryStrategy ResponsePoll { get; set; } = new ConstantBackoff {MaxAttemptNumber = 4, Interval = TimeSpan.FromSeconds(0.1)};
}
