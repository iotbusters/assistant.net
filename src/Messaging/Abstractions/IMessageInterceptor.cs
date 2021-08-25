using System;
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
        Task<TResponse> Intercept(TMessage message, Func<TMessage, Task<TResponse>> next);

        Task<object> IAbstractInterceptor.Intercept(object message, Func<object, Task<object>> next) =>
            Intercept((TMessage) message, x => next(x).MapSuccess(y => (TResponse) y)).MapSuccess(y => (object) y!);
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
        Task Intercept(TMessage message, Func<TMessage, Task> next);

        async Task<None> IMessageInterceptor<TMessage, None>.Intercept(TMessage message, Func<TMessage, Task<None>> next)
        {
            await Intercept(message, next);
            return None.Instance;
        }
    }

    /// <summary>
    ///     Simple alias to interceptor that can handle all types of messages.
    /// </summary>
    public interface IMessageInterceptor : IMessageInterceptor<IMessage<object>, object>
    {
    }
}