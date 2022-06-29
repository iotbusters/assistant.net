using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Base implementation of shared message handling interceptor of published and requested messages.
/// </summary>
public abstract class SharedAbstractInterceptor : IAbstractRequestInterceptor, IAbstractPublishInterceptor
{
    /// <inheritdoc/>
    public async ValueTask<object> Intercept(RequestMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        await InterceptInternal((m, t) => next(m, t), message, token);

    /// <inheritdoc/>
    public async ValueTask Intercept(PublishMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        await InterceptInternal(async (m, t) =>
        {
            await next(m, t);
            return Nothing.Instance;
        }, message, token);

    /// <summary>
    ///     Executes single request/publish custom behavior over <paramref name="next"/> message handler callback.
    /// </summary>
    protected abstract ValueTask<object> InterceptInternal(SharedMessageHandler next, IAbstractMessage message, CancellationToken token);
}

/// <summary>
///     A function requesting a message.
/// </summary>
public delegate ValueTask<object> SharedMessageHandler(IAbstractMessage message, CancellationToken token);
