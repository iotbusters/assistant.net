using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Timeout tracking interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.Timeout"/>
/// </remarks>
public sealed class TimeoutInterceptor : SharedAbstractInterceptor
{
    private readonly ILogger<TimeoutInterceptor> logger;
    private readonly ITypeEncoder typeEncode;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public TimeoutInterceptor(
        ILogger<TimeoutInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncode = typeEncode;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    /// <exception cref="TimeoutException"/>
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        logger.LogInformation("Message timeout counter: begins.");

        var timeout = Debugger.IsAttached
            ? Timeout.InfiniteTimeSpan
            : options.Timeout;
        using var timeoutSource = new CancellationTokenSource(timeout);
        using var compositeSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, token);

        var watch = Stopwatch.StartNew();
        object response;
        try
        {
            response = await next(message, compositeSource.Token);
            watch.Stop();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogWarning("Message timeout counter: cancelled in {RunTime}.", watch.Elapsed);
            throw;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            var runtime = watch.Elapsed;
            logger.LogError("Message timeout counter: {RunTime} exceeded the {Timeout} limit.", runtime, timeout);
            throw new TimeoutException($"Operation run for {runtime} and exceeded the {timeout} limit.");
        }

        logger.LogInformation("Message timeout counter: ends in {RunTime}.", watch.Elapsed);
        return response;
    }
}
