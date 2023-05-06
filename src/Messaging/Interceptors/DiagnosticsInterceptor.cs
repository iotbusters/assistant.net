using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
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
    private readonly ITypeEncoder typeEncoder;
    private readonly IDiagnosticFactory diagnosticFactory;

    /// <summary/>
    public DiagnosticsInterceptor(
        ILogger<DiagnosticsInterceptor> logger,
        ITypeEncoder typeEncoder,
        IDiagnosticFactory diagnosticFactory)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.diagnosticFactory = diagnosticFactory;
    }

    /// <inheritdoc/>
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType());

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
        catch (Exception)
        {
            logger.LogInformation("Message handling operation: failed.");
            operation.Fail();
            throw;
        }

        logger.LogInformation("Message handling operation: ends.");
        operation.Complete();
        return response;
    }
}
