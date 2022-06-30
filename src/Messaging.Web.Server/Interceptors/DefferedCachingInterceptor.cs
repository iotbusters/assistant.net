using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
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
public sealed class DeferredCachingInterceptor : SharedAbstractInterceptor
{
    private static readonly ConcurrentDictionary<string, DeferredCachingResult<object>> deferredCache = new();

    private readonly ILogger logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public DeferredCachingInterceptor(
        ILogger<DeferredCachingInterceptor> logger,
        ITypeEncoder typeEncoder,
        INamedOptions<MessagingClientOptions> options) : this((ILogger)logger, typeEncoder, options) { }

    /// <summary/>
    public DeferredCachingInterceptor(
        ILogger logger,
        ITypeEncoder typeEncoder,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());
        logger.LogInformation("Message({MessageName}/{MessageId}) deferred caching: begins.", messageName, messageId);

        return await deferredCache.GetOrAdd(message.GetSha1(), _ => StartIntercepting(next, message, token)).GetTask();
    }

    private async Task<object> StartIntercepting(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());
        object response;
        try
        {
            response = await next(message, token);
        }
        catch (Exception ex)
        {
            if (ex is MessageDeferredException || options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
            {
                deferredCache.TryRemove(message.GetSha1(), out _);
                logger.LogError(ex, "Message({MessageName}/{MessageId}) deferred caching: rethrows transient failure.",
                    messageName, messageId);
            }
            else
                logger.LogWarning(ex, "Message({MessageName}/{MessageId}) deferred caching: rethrows permanent failure.",
                    messageName, messageId);
            throw;
        }

        logger.LogInformation("Message({MessageName}/{MessageId}) deferred caching: ends.", messageName, messageId);
        return response;
    }
}
