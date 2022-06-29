using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Graceful shutdown strategy interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.CancellationDelay"/>
/// </remarks>
public class CancellationDelayInterceptor : SharedAbstractInterceptor
{
    private readonly ILogger<CancellationDelayInterceptor> logger;
    private readonly ITypeEncoder typeEncode;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public CancellationDelayInterceptor(
        ILogger<CancellationDelayInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncode = typeEncode;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    protected override async ValueTask<object> InterceptInternal(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        logger.LogInformation("Message({MessageName}/{MessageId}) timeout counter: begins.", messageName, messageId);

        using var gracefulShutdownSource = new CancellationTokenSource();
        await using var registration = token.Register(() => gracefulShutdownSource.CancelAfter(options.CancellationDelay));

        var timer = new Stopwatch();
        object response;
        try
        {
            response = await next(message, gracefulShutdownSource.Token);
        }
        catch (OperationCanceledException ex) when (gracefulShutdownSource.IsCancellationRequested)
        {
            var delayedTime = timer.Elapsed;
            var delayTime = options.CancellationDelay;
            logger.LogError(ex, "Message({MessageName}/{MessageId}) cancellation delay: cancelled hardly after {DelayedTime} "
                                + "exceeded the {DelayTime} limit.",
                messageName, messageId, delayedTime, delayTime);
            throw new OperationCanceledException($"Operation was cancelled hardly after {delayedTime}.", ex);
        }

        if (timer.Elapsed > TimeSpan.Zero)
            logger.LogInformation("Message({MessageName}/{MessageId}) cancellation delay: ended gracefully in {DelayedTime}.",
                messageName, messageId, timer.Elapsed);
        else
            logger.LogInformation("Message({MessageName}/{MessageId}) cancellation delay: no cancellation requested.",
                messageName, messageId);

        return response;
    }
}
