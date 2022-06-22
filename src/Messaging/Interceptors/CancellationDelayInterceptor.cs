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
public class CancellationDelayInterceptor : IAbstractInterceptor
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
    public async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        logger.LogInformation("Message({MessageName}/{MessageId}) timeout counter: begins.", messageName, messageId);

        using var gracefulShutdownSource = new CancellationTokenSource();

        var timer = new Stopwatch();
        var gracefulShutdownTask = ConfigureGracefulShutdown(timer, gracefulShutdownSource, token);

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
        finally
        {
            gracefulShutdownSource.Cancel();
            await gracefulShutdownTask;
        }

        if (timer.Elapsed > TimeSpan.Zero)
            logger.LogInformation("Message({MessageName}/{MessageId}) cancellation delay: ended gracefully in {DelayedTime}.",
                messageName, messageId, timer.Elapsed);
        else
            logger.LogInformation("Message({MessageName}/{MessageId}) cancellation delay: no cancellation requested.",
                messageName, messageId);
        return response;
    }

    private async Task ConfigureGracefulShutdown(
        Stopwatch timer,
        CancellationTokenSource gracefulShutdownTokenSource,
        CancellationToken shutdownToken)
    {
        await Task.WhenAny(
            Task.Delay(Timeout.InfiniteTimeSpan, shutdownToken),
            Task.Delay(Timeout.InfiniteTimeSpan, gracefulShutdownTokenSource.Token));

        if (gracefulShutdownTokenSource.IsCancellationRequested)
            return;

        timer.Start();
        await Task.WhenAny(
            Task.Delay(options.CancellationDelay, gracefulShutdownTokenSource.Token));

        //gracefulShutdownSource.CancelAfter(options.CancellationDelay);
        gracefulShutdownTokenSource.Cancel();
    }
}

/// <inheritdoc cref="CancellationDelayInterceptor"/>
public sealed class CancellationDelayInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly CancellationDelayInterceptor interceptor;

    /// <summary/>
    public CancellationDelayInterceptor(
        ILogger<CancellationDelayInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options)
    {
        this.interceptor = new CancellationDelayInterceptor(logger, typeEncode, options);
    }

    /// <inheritdoc/>
    /// <exception cref="TimeoutException"/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
