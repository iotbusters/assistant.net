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

/// <inheritdoc cref="TimeoutInterceptor{TMessage,TResponse}"/>
public sealed class TimeoutInterceptor : TimeoutInterceptor<IMessage<object>, object>, IMessageInterceptor
{
    /// <summary/>
    public TimeoutInterceptor(
        ILogger<TimeoutInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options) : base(logger, typeEncode, options) { }
}

/// <summary>
///     Timeout tracking interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.Timeout"/>
/// </remarks>
public class TimeoutInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
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
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        logger.LogDebug("Message({MessageName}/{MessageId}) timeout counter: begins.", messageName, messageId);

        using var timeoutSource = new CancellationTokenSource(options.Timeout);
        using var compositeSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, token);

        var watch = Stopwatch.StartNew();
        TResponse response;
        try
        {
            response = await next(message, compositeSource.Token);
            watch.Stop();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogWarning("Message({MessageName}/{MessageId}) timeout counter: cancelled in {RunTime}.",
                messageName, messageId, watch.Elapsed);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            var runtime = watch.Elapsed;
            logger.LogError(ex, "Message({MessageName}/{MessageId}) timeout counter: {RunTime} exceeded the {Timeout} limit.",
                messageName, messageId, runtime, options.Timeout);
            throw new TimeoutException($"Operation run for {runtime} and exceeded the {options.Timeout} limit.", ex);
        }

        logger.LogDebug("Message({MessageName}/{MessageId}) timeout counter: ends in {RunTime}.",
            messageName, messageId, watch.Elapsed);
        return response;
    }
}
