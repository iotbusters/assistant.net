using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message interceptor abstraction that accepts a requested <typeparamref name="TMessage"/> and its children.
///     It's one piece in an intercepting chain with control over message and response.
/// </summary>
public interface IMessageRequestInterceptor<TMessage, TResponse> where TMessage : IMessage<TResponse>
{
    /// <summary>
    ///     Intercepts a requested <paramref name="message"/> or one of its children
    ///     and delegates the call to the <paramref name="next"/> interceptor if needed.
    /// </summary>
    ValueTask<TResponse> Intercept(RequestMessageHandler<TMessage, TResponse> next, TMessage message, CancellationToken token = default);
}

/// <summary>
///     A function requesting a message.
/// </summary>
public delegate ValueTask<TResponse> RequestMessageHandler<in TMessage, TResponse>(TMessage message, CancellationToken token) where TMessage : IMessage<TResponse>;
