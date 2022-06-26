using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Operation tracking interceptor.
/// </summary>
public sealed class DiagnosticsInterceptor : IAbstractInterceptor
{
    private readonly ILogger logger;
    private readonly ITypeEncoder typeEncode;
    private readonly IDiagnosticFactory diagnosticFactory;

    /// <summary/>
    public DiagnosticsInterceptor(
        ILogger<DiagnosticsInterceptor> logger,
        ITypeEncoder typeEncode,
        IDiagnosticFactory diagnosticFactory)
    {
        this.logger = logger;
        this.typeEncode = typeEncode;
        this.diagnosticFactory = diagnosticFactory;
    }

    /// <inheritdoc/>
    public async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        var operation = diagnosticFactory.Start($"{messageName}-handling-local");
        logger.LogInformation("Message({MessageName}/{MessageId}) operation: begins.", messageId, messageName);

        object response;
        try
        {
            response = await next(message, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogWarning("Message({MessageName}/{MessageId}) operation: cancelled.",
                messageName, messageId);
            operation.Fail();
            throw;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Message({MessageName}/{MessageId}) operation: failed.", messageId, messageName);
            operation.Fail();
            throw;
        }

        logger.LogInformation("Message({MessageName}/{MessageId}) operation: succeeded.", messageId, messageName);
        operation.Complete();
        return response;
    }
}

/// <inheritdoc cref="DiagnosticsInterceptor"/>
public sealed class DiagnosticsInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly DiagnosticsInterceptor interceptor;

    /// <summary/>
    public DiagnosticsInterceptor(
        ILogger<DiagnosticsInterceptor> logger,
        ITypeEncoder typeEncode,
        IDiagnosticFactory diagnosticFactory)
    {
        this.interceptor = new DiagnosticsInterceptor(logger, typeEncode, diagnosticFactory);
    }

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(MessageInterceptor<TMessage, TResponse> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
