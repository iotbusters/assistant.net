using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message interceptor abstraction that accepts <typeparamref name="TMessage"/> and its children.
///     It's one piece in an intercepting chain with control over message and response.
/// </summary>
public interface IMessageInterceptor<TMessage, TResponse> where TMessage : IMessage<TResponse>
{
    /// <summary>
    ///     Intercepts the <paramref name="message"/> or one of its children
    ///     and delegates the call to the <paramref name="next"/> interceptor if needed.
    /// </summary>
    Task<TResponse> Intercept(MessageInterceptor<TMessage, TResponse> next, TMessage message, CancellationToken token = default);
}

/// <summary>
///     Message interceptor abstraction that accepts <typeparamref name="TMessage"/> and its children.
///     It's one piece in an intercepting chain with control over message with no response expectation.
/// </summary>
public interface IMessageInterceptor<TMessage> : IMessageInterceptor<TMessage, Nothing> where TMessage : IMessage
{
    /// <summary>
    ///     Intercepts the <paramref name="message"/> or one of its children
    ///     and delegates the call to the <paramref name="next"/> interceptor if needed.
    /// </summary>
    Task Intercept(Func<TMessage, CancellationToken, Task> next, TMessage message, CancellationToken token = default);

    [StackTraceHidden]
    async Task<Nothing> IMessageInterceptor<TMessage, Nothing>.Intercept(MessageInterceptor<TMessage, Nothing> next, TMessage message, CancellationToken token)
    {
        await Intercept(next, message, token);
        return Nothing.Instance;
    }
}

/// <summary>
///     A function handling the message.
/// </summary>
public delegate Task<TResponse> MessageInterceptor<in TMessage, TResponse>(TMessage message, CancellationToken token) where TMessage : IMessage<TResponse>;
