using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     MongoDB client configuration used for remote message handling coordination.
/// </summary>
public class MongoHandlingClientOptions
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
