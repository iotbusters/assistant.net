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
public sealed class DiagnosticsInterceptor : SharedAbstractInterceptor
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
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncode.Encode(message.GetType());

        using var scope = logger.BeginPropertyScope()
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName);

        var operation = diagnosticFactory.Start($"{messageName}-handling-local");
        logger.LogInformation("Message handling operation: begins.");

        object response;
        try
        {
            response = await next(message, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogWarning("Message handling operation: cancelled.");
            operation.Fail();
            throw;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Message handling operation: failed.");
            operation.Fail();
            throw;
        }

        logger.LogInformation("Message handling operation: ends.");
        operation.Complete();
        return response;
    }
}
