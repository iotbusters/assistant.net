using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class LocalMessageHandlingProxy<TMessage, TResponse> : IAbstractHandler
    where TMessage : IMessage<TResponse>
{
    private readonly ILogger<LocalMessageHandlingProxy<TMessage, TResponse>> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IMessageHandler<TMessage, TResponse> handler;

    public LocalMessageHandlingProxy(
        ILogger<LocalMessageHandlingProxy<TMessage, TResponse>> logger,
        ITypeEncoder typeEncoder,
        IMessageHandler<TMessage, TResponse> handler)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.handler = handler;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        using var scope = logger.BeginPropertyScope()
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName);

        logger.LogDebug("Message handling: begins.");

        var response = (await handler.Handle((TMessage)message, token))!;

        logger.LogDebug("Message handling: ends.");

        return response;
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        using var scope = logger.BeginPropertyScope()
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName);

        logger.LogDebug("Message publishing: begins.");

        await handler.Handle((TMessage)message, token);

        logger.LogInformation("Message publishing: ends.");
    }
}
