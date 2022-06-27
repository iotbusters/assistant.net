using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Deferred message response (including failures) caching interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>
/// </remarks>
public sealed class DeferredCachingInterceptor : IAbstractInterceptor
{
    private static readonly ConcurrentDictionary<string, DeferredCachingResult<object>> deferredCache = new();

    private readonly MessagingClientOptions options;

    /// <summary/>
    public DeferredCachingInterceptor(INamedOptions<MessagingClientOptions> options) =>
        this.options = options.Value;

    /// <inheritdoc/>
    public async Task<object> Intercept(MessageInterceptor next, IAbstractMessage message, CancellationToken token) =>
        await deferredCache.GetOrAdd(message.GetSha1(), _ => StartIntercepting(next, message, token)).GetTask();

    private async Task<object> StartIntercepting(MessageInterceptor next, IAbstractMessage message, CancellationToken token)
    {
        try
        {
            return await next(message, token);
        }
        catch (Exception ex)
        {
            if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                deferredCache.TryRemove(message.GetSha1(), out _);
            throw;
        }
    }
}

/// <inheritdoc cref="DeferredCachingInterceptor"/>
public class DeferredCachingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly DeferredCachingInterceptor interceptor;

    /// <summary/>
    public DeferredCachingInterceptor(INamedOptions<MessagingClientOptions> options) =>
        this.interceptor = new DeferredCachingInterceptor(options);

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(MessageInterceptor<TMessage, TResponse> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
