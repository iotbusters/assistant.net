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
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        logger.LogInformation("Message({MessageName}, {MessageId}) timeout counter: begins.", messageName, messageId);

        using var timeoutSource = new CancellationTokenSource(options.Timeout);
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
            logger.LogWarning("Message({MessageName}, {MessageId}) timeout counter: cancelled in {RunTime}.",
                messageName, messageId, watch.Elapsed);
            throw;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            var runtime = watch.Elapsed;
            logger.LogError("Message({MessageName}, {MessageId}) timeout counter: {RunTime} exceeded the {Timeout} limit.",
                messageName, messageId, runtime, options.Timeout);
            throw new TimeoutException($"Operation run for {runtime} and exceeded the {options.Timeout} limit.");
        }

        logger.LogInformation("Message({MessageName}, {MessageId}) timeout counter: ends in {RunTime}.",
            messageName, messageId, watch.Elapsed);
        return response;
    }
}
