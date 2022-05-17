using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <inheritdoc cref="DiagnosticsInterceptor{TMessage,TResponse}"/>
public class DiagnosticsInterceptor : DiagnosticsInterceptor<IMessage<object>, object>, IMessageInterceptor
{
    /// <summary/>
    public DiagnosticsInterceptor(ILogger<DiagnosticsInterceptor<IMessage<object>, object>> logger, IDiagnosticFactory diagnosticFactory) : base(logger, diagnosticFactory) { }
}

/// <summary>
///     Operation tracking interceptor.
/// </summary>
public class DiagnosticsInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly ILogger logger;
    private readonly IDiagnosticFactory diagnosticFactory;

    /// <summary/>
    public DiagnosticsInterceptor(
        ILogger<DiagnosticsInterceptor<TMessage, TResponse>> logger,
        IDiagnosticFactory diagnosticFactory)
    {
        this.logger = logger;
        this.diagnosticFactory = diagnosticFactory;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
    {
        var messageName = message.GetType().Name.ToLower();

        logger.LogInformation("{Message} handling operation has started.", messageName);
        var operation = diagnosticFactory.Start($"{messageName}-handling-local");

        try
        {
            var response = await next(message, token);
            logger.LogInformation("{Message} handling operation has succeeded.", messageName);
            operation.Complete();
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message} handling operation has failed.", messageName);
            operation.Fail();
            throw;
        }
    }
}