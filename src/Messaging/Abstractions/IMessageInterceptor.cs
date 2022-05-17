using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message interceptor abstraction that accepts <typeparamref name="TMessage" /> and its children.
///     It's one piece in an intercepting chain with control over message and response.
/// </summary>
public interface IMessageInterceptor<TMessage, TResponse> where TMessage : IMessage<TResponse>
{
    /// <summary>
    ///     Intercepts the <paramref name="message" /> or one of its children
    ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
    /// </summary>
    Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token = default);
}

/// <summary>
///     Message interceptor abstraction that accepts <typeparamref name="TMessage" /> and its children.
///     It's one piece in an intercepting chain with control over message with no response expectation.
/// </summary>
public interface IMessageInterceptor<TMessage> : IMessageInterceptor<TMessage, None> where TMessage : IMessage
{
    /// <summary>
    ///     Intercepts the <paramref name="message" /> or one of its children
    ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
    /// </summary>
    Task Intercept(IMessageHandler<TMessage> next, TMessage message, CancellationToken token = default);

    async Task<None> IMessageInterceptor<TMessage, None>.Intercept(Func<TMessage, CancellationToken, Task<None>> next, TMessage message, CancellationToken token)
    {
        var handler = next as IMessageHandler<TMessage> ?? new MessageHandlerAdapter(next);
        await Intercept(handler, message, token);
        return None.Instance;
    }

    private class MessageHandlerAdapter : IMessageHandler<TMessage>
    {
        private readonly Func<TMessage, CancellationToken, Task<None>> handler;

        public MessageHandlerAdapter(Func<TMessage, CancellationToken, Task<None>> handler) =>
            this.handler = handler;

        public Task Handle(TMessage message, CancellationToken token = default) =>
            handler(message, token);
    }
}

/// <summary>
///     Simple alias to interceptor that can handle all types of messages.
/// </summary>
public interface IMessageInterceptor : IMessageInterceptor<IMessage<object>, object> { }