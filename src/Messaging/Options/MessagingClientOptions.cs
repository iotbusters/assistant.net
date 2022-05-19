using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Interceptors;
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
    ///     List of registered handlers.
    /// </summary>
    public IDictionary<Type, HandlerDefinition> Handlers { get; } = new Dictionary<Type, HandlerDefinition>();

    /// <summary>
    ///     List of registered interceptors.
    /// </summary>
    public IList<InterceptorDefinition> Interceptors { get; } = new List<InterceptorDefinition>();

    /// <summary>
    ///     List of allowed for exposing external exceptions.
    /// </summary>
    /// <remarks>
    ///     Impacts <see cref="ErrorHandlingInterceptor{TMessage,TResponse}"/>.
    /// </remarks>
    public IList<Type> ExposedExceptions { get; } = new List<Type>();

    /// <summary>
    ///     List of allowed for retrying transient exceptions.
    /// </summary>
    /// <remarks>
    ///     Impacts <see cref="CachingInterceptor{TMessage,TResponse}"/> and <see cref="RetryingInterceptor{TMessage,TResponse}"/>.
    /// </remarks>
    public IList<Type> TransientExceptions { get; } = new List<Type>();

    /// <summary>
    ///     Message handling retry strategy.
    /// </summary>
    /// <remarks>
    ///     Impacts <see cref="RetryingInterceptor{TMessage,TResponse}"/>.
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
    ///     Impacts <see cref="TimeoutInterceptor{TMessage,TResponse}"/>.
    /// </remarks>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
