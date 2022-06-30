using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Base implementation of shared message handling interceptor of published and requested messages.
/// </summary>
[StackTraceHidden]
public abstract class SharedAbstractInterceptor : IAbstractRequestInterceptor, IAbstractPublishInterceptor
{
    /// <inheritdoc/>
    public async ValueTask<object> Intercept(RequestMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        await Intercept(new SharedMessageHandler((m, t) => next(m, t)), message, token);

    /// <inheritdoc/>
    public async ValueTask Intercept(PublishMessageHandler next, IAbstractMessage message, CancellationToken token) =>
        await Intercept(new SharedMessageHandler(async(m, t) =>
        {
            await next(m, t);
            return Nothing.Instance;
        }), message, token);

    /// <summary>
    ///     Executes single request/publish custom behavior over <paramref name="next"/> message handler callback.
    /// </summary>
    protected abstract ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token);
}

/// <summary>
///     A function requesting a message.
/// </summary>
public delegate ValueTask<object> SharedMessageHandler(IAbstractMessage message, CancellationToken token);
