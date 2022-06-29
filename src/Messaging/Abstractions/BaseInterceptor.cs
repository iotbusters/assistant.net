using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Base implementation of an adapter between <see cref="SharedAbstractInterceptor"/>
///     and <see cref="IMessageRequestInterceptor{TMessage,TResponse}"/>.
/// </summary>
public abstract class BaseInterceptor<TMessage, TResponse> : IMessageRequestInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly SharedAbstractInterceptor interceptor;

    /// <summary/>
    protected BaseInterceptor(SharedAbstractInterceptor interceptor) =>
        this.interceptor = interceptor;

    /// <inheritdoc/>
    public virtual async ValueTask<TResponse> Intercept(RequestMessageHandler<TMessage, TResponse> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}

/// <summary>
///     Base implementation of an adapter between <see cref="SharedAbstractInterceptor"/>
///     and <see cref="IMessagePublishInterceptor{TMessage}"/>.
/// </summary>
public abstract class BaseInterceptor<TMessage> : IMessagePublishInterceptor<TMessage>
    where TMessage : IAbstractMessage
{
    private readonly SharedAbstractInterceptor interceptor;

    /// <summary/>
    protected BaseInterceptor(SharedAbstractInterceptor interceptor) =>
        this.interceptor = interceptor;

    /// <inheritdoc/>
    public virtual async ValueTask Intercept(PublishMessageHandler<TMessage> next, TMessage message, CancellationToken token) =>
        await interceptor.Intercept(async (m, t) => await next((TMessage)m, t), message, token);
}
