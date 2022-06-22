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

/// <summary>
///     Global error handling interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.ExposedExceptions"/>
/// </remarks>
public sealed class ErrorHandlingInterceptor : IAbstractInterceptor
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
    /// <exception cref="MessageFailedException"/>
    public async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        logger.LogInformation("Message({MessageName}/{MessageId}) error handling: begins.", messageName, messageId);

        object response;
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

        logger.LogInformation("Message({MessageName}/{MessageId}) error handling: ends.", messageName, messageId);
        return response;
    }
}

/// <inheritdoc cref="DiagnosticsInterceptor"/>
public sealed class ErrorHandlingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly ErrorHandlingInterceptor interceptor;

    /// <summary/>
    public ErrorHandlingInterceptor(
        ILogger<ErrorHandlingInterceptor> logger,
        ITypeEncoder typeEncode,
        INamedOptions<MessagingClientOptions> options)
    {
        this.interceptor = new ErrorHandlingInterceptor(logger, typeEncode, options);
    }

    /// <inheritdoc/>
    /// <exception cref="MessageFailedException"/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
