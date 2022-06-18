using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <inheritdoc cref="CachingInterceptor{TMessage,TResponse}"/>
public sealed class CachingInterceptor : CachingInterceptor<IMessage<object>, object>, IMessageInterceptor
{
    /// <summary/>
    public CachingInterceptor(IStorage<IAbstractMessage, CachingResult> cache, INamedOptions<MessagingClientOptions> options) : base(cache, options) { }
}

/// <summary>
///     Message response (including failures) caching interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>,
///     <see cref="IMessageCacheIgnored"/>.
/// </remarks>
public class CachingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly IStorage<IAbstractMessage, CachingResult> cache;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public CachingInterceptor(IStorage<IAbstractMessage, CachingResult> cache, INamedOptions<MessagingClientOptions> options)
    {
        this.cache = cache;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
    {
        if(message is IMessageCacheIgnored)
            return await next(message, token);

        var result = await cache.AddOrGet(message, async _ =>
        {
            try
            {
                var response = await next(message, token);
                return CachingResult.OfValue((dynamic)response!);
            }
            catch (Exception ex)
            {
                if (ex is not MessageDeferredException && !options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                    return CachingResult.OfException(ex);
                throw;
            }
        }, token);

        return (TResponse)result.GetValue();
    }
}
