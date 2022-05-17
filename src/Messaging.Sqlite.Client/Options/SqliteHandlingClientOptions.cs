using Assistant.Net.Abstractions;
using Assistant.Net.RetryStrategies;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     SQLite client configuration used for remote message handling coordination.
/// </summary>
public class SqliteHandlingClientOptions
{
    /// <summary>
    ///     Message handling response polling strategy.
    /// </summary>
    public IRetryStrategy ResponsePoll { get; set; } = new ConstantBackoff {MaxAttemptNumber = 4, Interval = TimeSpan.FromSeconds(0.1)};
}