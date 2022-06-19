using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <inheritdoc cref="CachingInterceptor{TMessage,TResponse}"/>
public class CachingInterceptor : CachingInterceptor<IMessage<object>, object>, IMessageInterceptor
{
    /// <summary/>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options)
        : base(logger, typeEncoder, cache, options) { }
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
    private readonly ILogger<CachingInterceptor> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IStorage<IAbstractMessage, CachingResult> cache;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public CachingInterceptor(
        ILogger<CachingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.cache = cache;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
    {
        if(message is IMessageCacheIgnored)
            return await next(message, token);

        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());

        logger.LogDebug("Message({MessageName}/{MessageId}) caching: begins.", messageName, messageId);

        var result = await cache.AddOrGet(message, async _ =>
        {
            CachingResult result;
            try
            {
                var response = await next(message, token);
                result = CachingResult.OfValue((dynamic)response!);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                logger.LogWarning("Message({MessageName}/{MessageId}) caching: cancelled.", messageName, messageId);
                throw;
            }
            catch (Exception ex)
            {
                if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                {
                    logger.LogError(ex, "Message({MessageName}/{MessageId}) caching: rethrows transient failure.", messageName, messageId);
                    throw;
                }

                logger.LogError(ex, "Message({MessageName}/{MessageId}) caching: accepts permanent failure.", messageName, messageId);
                result = CachingResult.OfException(ex);
            }

            logger.LogDebug("Message({MessageName}/{MessageId}) caching: success.", messageName, messageId);
            return result;
        }, token);

        logger.LogDebug("Message({MessageName}/{MessageId}) caching: ends.", messageName, messageId);
        return (TResponse)result.GetValue();
    }
}
