using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Options;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Storage based message handling client configuration.
/// </summary>
public sealed class GenericHandlerProxyOptions
{
    /// <summary>
    ///     Message handling response polling strategy.
    /// </summary>
    public IRetryStrategy ResponsePoll { get; internal set; } = null!;

    /// <summary>
    ///     Instance factory using for message handling host selection strategy.
    /// </summary>
    public InstanceFactory<IHostSelectionStrategy> HostSelectionStrategyFactory { get; internal set; } = null!;
}
