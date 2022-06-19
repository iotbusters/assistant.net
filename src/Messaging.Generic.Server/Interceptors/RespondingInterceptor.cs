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

/// <summary>
///     Message response (including failures) storing interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/>.
///     It duplicates <see cref="CachingInterceptor"/> except <see cref="IMessageCacheIgnored"/> marker support.
/// </remarks>
internal class RespondingInterceptor : IMessageInterceptor
{
    private readonly ILogger<RespondingInterceptor> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IStorage<IAbstractMessage, CachingResult> cache;
    private readonly MessagingClientOptions options;

    public RespondingInterceptor(
        ILogger<RespondingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IStorage<IAbstractMessage, CachingResult> cache,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.cache = cache;
        this.options = options.Value;
    }

    public async Task<object> Intercept(Func<IMessage<object>, CancellationToken, Task<object>> next, IMessage<object> message, CancellationToken token = default)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());

        logger.LogDebug("Message({MessageName}/{MessageId}) responding: begins.", messageName, messageId);

        var result = await cache.AddOrGet(message, async _ =>
        {
            CachingResult result;
            try
            {
                var response = await next(message, token);
                result = CachingResult.OfValue((dynamic)response);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                logger.LogWarning("Message({MessageName}/{MessageId}) responding: cancelled.", messageName, messageId);
                throw;
            }
            catch (Exception ex)
            {
                if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                {
                    logger.LogError(ex, "Message({MessageName}/{MessageId}) responding: rethrows transient failure.",
                        messageName, messageId);
                    throw;
                }

                logger.LogInformation(ex, "Message({MessageName}/{MessageId}) responding: failure.", messageName, messageId);
                result = CachingResult.OfException(ex);
            }

            logger.LogInformation("Message({MessageName}/{MessageId}) responding: success.", messageName, messageId);
            return result;
        }, token);

        logger.LogDebug("Message({MessageName}/{MessageId}) responding: ends.", messageName, messageId);
        return result.GetValue();
    }
}
