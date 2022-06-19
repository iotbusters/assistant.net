using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <inheritdoc cref="DiagnosticsInterceptor{TMessage,TResponse}"/>
public sealed class ErrorHandlingInterceptor : ErrorHandlingInterceptor<IMessage<object>, object>, IMessageInterceptor
{
    /// <summary/>
    public ErrorHandlingInterceptor(
        ILogger<ErrorHandlingInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options) : base(logger, typeEncode, options) { }
}

/// <summary>
///     Global error handling interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.ExposedExceptions"/>
/// </remarks>
public class ErrorHandlingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly ILogger<ErrorHandlingInterceptor> logger;
    private readonly ITypeEncoder typeEncode;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public ErrorHandlingInterceptor(
        ILogger<ErrorHandlingInterceptor> logger,
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

        logger.LogDebug("Message({MessageName}/{MessageId}) error handling: begins.", messageName, messageId);

        TResponse response;
        try
        {
            response = await next(message, token);
        }
        catch (OperationCanceledException ex) when (token.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Message({MessageName}/{MessageId}) error handling: cancelled.", messageName, messageId);
            throw;
        }
        catch (Exception ex)
        {
            if (options.ExposedExceptions.Any(x => x.IsInstanceOfType(ex)))
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}) error handling: rethrow to expose.", messageName, messageId);
                throw;
            }

            logger.LogError(ex, "Message({MessageName}/{MessageId}) error handling: wraps internal to hide.", messageName, messageId);
            throw new MessageFailedException(ex);
        }

        logger.LogDebug("Message({MessageName}/{MessageId}) error handling: ends.", messageName, messageId);
        return response;
    }
}
