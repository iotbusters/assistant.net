using Assistant.Net.Abstractions;
using Assistant.Net.DataAnnotations;
using Assistant.Net.Messaging.Abstractions;
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
    ///     A single message handler instance factory using for message handling configured for the feature.
    /// </summary>
    public InstanceFactory<IAbstractHandler, Type>? SingleHandlerFactory { get; internal set; }

    /// <summary>
    ///     A message handler instance factory using for not registered in <see cref="HandlerFactories"/> message types.
    /// </summary>
    public InstanceFactory<IAbstractHandler, Type>? BackoffHandlerFactory { get; internal set; }

    /// <summary>
    ///     Specific message type handler registrations.
    /// </summary>
    public Dictionary<Type, InstanceFactory<IAbstractHandler>> HandlerFactories { get; } = new();

    /// <summary>
    ///     Request message operation interceptor registrations.
    /// </summary>
    public List<InterceptorDefinition<IAbstractRequestInterceptor>> RequestInterceptors { get; } = new();
    
    /// <summary>
    ///     Publish message operation interceptor registrations.
    /// </summary>
    public List<InterceptorDefinition<IAbstractPublishInterceptor>> PublishInterceptors { get; } = new();

    /// <summary>
    ///     Allowed for exposing external exception registrations.
    /// </summary>
    /// <remarks>
    ///     It impacts <see cref="ErrorHandlingInterceptor"/>.
    /// </remarks>
    public HashSet<Type> ExposedExceptions { get; } = new();

    /// <summary>
    ///     Allowed for retrying transient exception registrations.
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
    [Time("00:00:00.000001", "00:30:00", AllowInfinite = true)]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}
