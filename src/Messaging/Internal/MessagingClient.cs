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

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Default messaging client implementation.
/// </summary>
internal class MessagingClient : IMessagingClient
{
    private readonly ILogger<MessagingClient> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IServiceProvider provider;
    private readonly INamedOptions<MessagingClientOptions> options;

    public MessagingClient(
        ILogger<MessagingClient> logger,
        ITypeEncoder typeEncoder,
        IServiceProvider provider,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.provider = provider;
        this.options = options;
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public async Task<object> RequestObject(IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());
        using var _ = logger.BeginPropertyScope()
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName);

        var clientOptions = options.Value;

        var messageType = message.GetType();
        if (!clientOptions.HandlerFactories.TryGetValue(messageType, out var factory)
            && clientOptions.BackoffHandlerFactory == null)
            throw new MessageNotRegisteredException(messageType);

        var handler = factory?.Create(provider) ?? clientOptions.BackoffHandlerFactory!.Create(provider, messageType);
        var interceptors = clientOptions.RequestInterceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Factory.Create(provider));

        var messageHandler = new InterceptingRequestMessageHandler(handler, interceptors);
        return await messageHandler.Request(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public async Task PublishObject(IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());
        using var _ = logger.BeginPropertyScope()
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName);

        var clientOptions = options.Value;

        var messageType = message.GetType();
        if (!clientOptions.HandlerFactories.TryGetValue(messageType, out var factory)
            && clientOptions.BackoffHandlerFactory == null)
            throw new MessageNotRegisteredException(messageType);

        var handler = factory?.Create(provider) ?? clientOptions.BackoffHandlerFactory!.Create(provider, messageType);
        var interceptors = clientOptions.PublishInterceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Factory.Create(provider));

        var messageHandler = new InterceptingPublishMessageHandler(handler, interceptors);
        await messageHandler.Publish(message, token);
    }
}
