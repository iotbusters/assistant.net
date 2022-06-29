using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
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
        INamedOptions<MessagingClientOptions> options) : base((ILogger)logger, typeEncoder, cache, options) { }

    /// <inheritdoc/>
    protected override async ValueTask<object> InterceptInternal(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        if(message is INonCaching)
            return await next(message, token);

        return await base.InterceptInternal(next, message, token);
    }
}
