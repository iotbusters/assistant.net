using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Default message response (including failures) caching interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>.
/// </remarks>
public class DefaultCachingInterceptor : SharedAbstractInterceptor
{
    private readonly ILogger logger;
    private readonly IStorage<IAbstractMessage, CachingResult> cache;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public DefaultCachingInterceptor(
        ILogger<DefaultCachingInterceptor> logger,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options) : this((ILogger)logger, cache, options) { }

    /// <summary/>
    protected DefaultCachingInterceptor(
        ILogger logger,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.cache = cache;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        logger.LogInformation("Message caching: begins.");

        var result = await cache.AddOrGet(message, async _ =>
        {
            CachingResult result;
            try
            {
                var response = await next(message, token);
                result = CachingResult.OfValue((dynamic) response);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                logger.LogWarning("Message caching: cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                {
                    logger.LogError(ex, "Message caching: rethrows transient failure.");
                    throw;
                }

                logger.LogWarning(ex, "Message caching: ends with permanent failure.");
                result = CachingResult.OfException(ex);
            }

            logger.LogInformation("Message caching: ends with success response.");
            return result;
        }, token);

        logger.LogInformation("Message caching: ends.");
        return result.GetValue();
    }
}
