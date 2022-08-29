using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
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
    private readonly IServiceProvider provider;
    private readonly INamedOptions<MessagingClientOptions> options;

    public MessagingClient(IServiceProvider provider, INamedOptions<MessagingClientOptions> options)
    {
        this.provider = provider;
        this.options = options;
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public async Task<object> RequestObject(IAbstractMessage message, CancellationToken token)
    {
        var clientOptions = options.Value;

        var messageType = message.GetType();
        if (!clientOptions.Handlers.TryGetValue(messageType, out var factory)
            && clientOptions.AnyProvider == null)
            throw new MessageNotRegisteredException(messageType);

        var handler = (factory ?? clientOptions.AnyProvider!).Create(provider);
        var interceptors = clientOptions.RequestInterceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Factory.Create(provider));

        var messageHandler = new InterceptingRequestMessageHandler(handler, interceptors);
        return await messageHandler.Request(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public async Task PublishObject(IAbstractMessage message, CancellationToken token)
    {
        var clientOptions = options.Value;

        var messageType = message.GetType();
        if (!clientOptions.Handlers.TryGetValue(messageType, out var factory)
            && clientOptions.AnyProvider == null)
            throw new MessageNotRegisteredException(messageType);

        var handler = (factory ?? clientOptions.AnyProvider!).Create(provider);
        var interceptors = clientOptions.PublishInterceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Factory.Create(provider));

        var messageHandler = new InterceptingPublishMessageHandler(handler, interceptors);
        await messageHandler.Publish(message, token);
    }
}
