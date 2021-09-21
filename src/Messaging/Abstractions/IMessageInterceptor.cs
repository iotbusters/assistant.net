using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message interceptor abstraction that accepts <typeparamref name="TMessage" /> and its children.
    ///     It's one piece in an intercepting chain with control over message and response.
    /// </summary>
    public interface IMessageInterceptor<TMessage, TResponse> : IAbstractInterceptor
        where TMessage : IMessage<TResponse>
    {
        /// <summary>
        ///     Intercepts the <paramref name="message" /> or one of its children
        ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
        /// </summary>
        Task<TResponse> Intercept(
            Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token = default);

        async Task<object> IAbstractInterceptor.Intercept(
            Func<object, CancellationToken, Task<object>> next, object message, CancellationToken token) =>
            (await Intercept((m, t) => next(m, t).MapCompleted(x => (TResponse)x), (TMessage)message, token))!;
    }

    /// <summary>
    ///     Message interceptor abstraction that accepts <typeparamref name="TMessage" /> and its children.
    ///     It's one piece in an intercepting chain with control over message with no response expectation.
    /// </summary>
    public interface IMessageInterceptor<TMessage> : IMessageInterceptor<TMessage, None>
        where TMessage : IMessage<None>
    {
        /// <summary>
        ///     Intercepts the <paramref name="message" /> or one of its children
        ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
        /// </summary>
        Task Intercept(Func<TMessage, CancellationToken, Task> next, TMessage message, CancellationToken token = default);

        async Task<None> IMessageInterceptor<TMessage, None>.Intercept(
            Func<TMessage, CancellationToken, Task<None>> next, TMessage message, CancellationToken token)
        {
            await Intercept(next, message, token);
            return None.Instance;
        }
    }

    /// <summary>
    ///     Simple alias to interceptor that can handle all types of messages.
    /// </summary>
    public interface IMessageInterceptor : IMessageInterceptor<IMessage<object>, object> { }
}
