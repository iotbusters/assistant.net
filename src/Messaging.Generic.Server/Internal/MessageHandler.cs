using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Models;
using Assistant.Net.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal sealed class MessageHandler : IDisposable
{
    private readonly ILogger<MessageHandler> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IDisposable disposable;

    public MessageHandler(
        ILogger<MessageHandler> logger,
        ITypeEncoder typeEncoder,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.scopeFactory = scopeFactory;
        var scope = scopeFactory.CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        disposable = scope;
    }

    public async Task Handle(IAbstractMessage message, Audit audit, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        await using var scope = scopeFactory
            .CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName)
            .ConfigureDiagnosticContext(audit.CorrelationId, audit.User);

        var client = scope.ServiceProvider.GetRequiredService<IMessagingClient>();
        
        logger.LogDebug("Message({MessageName}/{MessageId}) handling: begins.", messageName, messageId);

        try
        {
            await client.RequestObject(message, token); // response is stored at RespondingInterceptor
        }
        catch (OperationCanceledException ex) when (token.IsCancellationRequested)
        {
            logger.LogDebug(ex, "Message({MessageName}/{MessageId}) handling: cancelled.", messageName, messageId);
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message({MessageName}/{MessageId}) handling: failed.", messageName, messageId);
        }

        logger.LogDebug("Message({MessageName}/{MessageId}) handling: succeeded.", messageName, messageId);
    }

    void IDisposable.Dispose() => disposable.Dispose();
}
