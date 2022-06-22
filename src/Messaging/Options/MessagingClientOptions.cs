using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Options;
using Assistant.Net.RetryStrategies;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Messaging client configurations used during message handling.
/// </summary>
public sealed class MessagingClientOptions
{
    /// <summary>
    ///     Single provider instance used for message handling configured for the feature.
    /// </summary>
    public InstanceFactory<IAbstractHandler>? SingleProvider { get; internal set; }

    /// <summary>
    ///     List of registered handlers.
    /// </summary>
    public Dictionary<Type, InstanceFactory<IAbstractHandler>> Handlers { get; } = new();

    /// <summary>
    ///     List of registered interceptors.
    /// </summary>
    public List<InterceptorDefinition> Interceptors { get; } = new();

    /// <summary>
    ///     List of allowed for exposing external exceptions.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public HashSet<Type> ExposedExceptions { get; } = new() {typeof(MessageException)};

    /// <summary>
    ///     List of allowed for retrying transient exceptions.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CachingInterceptor"/> and <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public HashSet<Type> TransientExceptions { get; } = new();

    /// <summary>
    ///     Message handling retry strategy.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="RetryingInterceptor"/>.
    /// </remarks>
    public IRetryStrategy Retry { get; set; } = new ExponentialBackoff
    {
        MaxAttemptNumber = 5,
        Interval = TimeSpan.FromSeconds(1),
        Rate = 1.2
    };

    /// <summary>
    ///     Message handling timeout.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="TimeoutInterceptor"/>.
    /// </remarks>
    [Time("00:00:00.001", "00:30:00")]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Delay between cancellation was requested and actually called.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="CancellationDelayInterceptor"/>.
    /// </remarks>
    [Time("00:00:00", "00:30:00")]
    public TimeSpan CancellationDelay { get; set; } = TimeSpan.FromSeconds(10);
}
