using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message interceptor abstraction that accepts a published <typeparamref name="TMessage"/> and its children.
///     It's one piece in an intercepting chain with control over message with no response expectation.
/// </summary>
public interface IMessagePublishInterceptor<TMessage> where TMessage : IAbstractMessage
{
    /// <summary>
    ///     Intercepts a publish <paramref name="message"/> or one of its children
    ///     and delegates the call to the <paramref name="next"/> interceptor if needed.
    /// </summary>
    ValueTask Intercept(PublishMessageHandler<TMessage> next, TMessage message, CancellationToken token = default);
}

/// <summary>
///     A function publishing a message.
/// </summary>
public delegate ValueTask PublishMessageHandler<in TMessage>(TMessage message, CancellationToken token) where TMessage : IAbstractMessage;
