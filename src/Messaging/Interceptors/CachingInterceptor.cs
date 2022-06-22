using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Message response (including failures) caching interceptor
///     which disables caching if message implements <see cref="INonCaching"/>.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>.
/// </remarks>
public sealed class CachingInterceptor : DefaultCachingInterceptor
{
    /// <summary/>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options) : base(logger, typeEncoder, cache, options) { }

    /// <inheritdoc/>
    public override async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token)
    {
        if(message is INonCaching)
            return await next(message, token);

        return await base.Intercept(next, message, token);
    }
}

/// <summary>
///     Message response (including failures) caching interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>,
///     <see cref="INonCaching"/>.
/// </remarks>
public sealed class CachingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly CachingInterceptor interceptor;

    /// <summary/>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options)
    {
        interceptor = new CachingInterceptor(logger, typeEncoder, cache, options);
    }

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
