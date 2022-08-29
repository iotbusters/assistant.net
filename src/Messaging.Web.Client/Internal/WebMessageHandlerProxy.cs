using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Strongly typed proxy to remote message handling.
/// </summary>
internal class WebMessageHandlerProxy : IAbstractHandler
{
    private readonly ILogger logger;
    private readonly IWebMessageHandlerClient client;

    public WebMessageHandlerProxy(
        ILogger<WebMessageHandlerProxy> logger,
        IWebMessageHandlerClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        var messageName = message.GetType().Name;
        var messageId = message.GetSha1();

        logger.LogInformation("Message({MessageName}, {MessageId}) handling: begins.", messageName, messageId);

        object response;
        try
        {
            response = await client.DelegateHandling(message, token);
        }
        catch (MessageDeferredException ex)
        {
            logger.LogInformation(ex, "Message({MessageName}, {MessageId}) handling: deferred.", messageName, messageId);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Message({MessageName}, {MessageId}) handling: cancelled or exceeded timeout.", messageName, messageId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message({MessageName}, {MessageId}) handling: failed.", messageName, messageId);
            throw;
        }

        logger.LogInformation("Message({MessageName}, {MessageId}) handling: ends.", messageName, messageId);
        return response;
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token)
    {
        var messageName = message.GetType().Name;
        var messageId = message.GetSha1();

        logger.LogInformation("Message({MessageName}, {MessageId}) publishing: begins.", messageName, messageId);

        try
        {
            await client.DelegateHandling(message, token);
        }
        catch (MessageDeferredException ex)
        {
            logger.LogInformation(ex, "Message({MessageName}, {MessageId}) publishing: ignore deferred.", messageName, messageId);
            return;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogInformation(ex, "Message({MessageName}, {MessageId}) publishing: cancelled or exceeded timeout.", messageName, messageId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message({MessageName}, {MessageId}) publishing: failed.", messageName, messageId);
            throw;
        }

        logger.LogInformation("Message({MessageName}, {MessageId}) publishing: ends.", messageName, messageId);
    }
}
