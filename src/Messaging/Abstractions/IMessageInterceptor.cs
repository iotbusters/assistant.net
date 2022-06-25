using System;
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
    Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token = default);
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

    async Task<Nothing> IMessageInterceptor<TMessage, Nothing>.Intercept(Func<TMessage, CancellationToken, Task<Nothing>> next, TMessage message, CancellationToken token)
    {
        await Intercept(next, message, token);
        return Nothing.Instance;
    }
}
