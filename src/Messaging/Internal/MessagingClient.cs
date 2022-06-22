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
    private readonly MessagingClientOptions options;

    public MessagingClient(IServiceProvider provider, INamedOptions<MessagingClientOptions> options)
    {
        this.provider = provider;
        this.options = options.Value;
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public Task<object> RequestObject(IAbstractMessage message, CancellationToken token)
    {
        var client = CreateInterceptingHandler(message.GetType());
        return client.Request(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public Task PublishObject(IAbstractMessage message, CancellationToken token)
    {
        var handler = CreateInterceptingHandler(message.GetType());
        return handler.Publish(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    private InterceptingMessageHandler CreateInterceptingHandler(Type messageType)
    {
        if(!options.Handlers.TryGetValue(messageType, out var factory))
            throw new MessageNotRegisteredException(messageType);

        var handler = factory.Create(provider);

        var interceptors = options.Interceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Factory.Create(provider));

        return new InterceptingMessageHandler(handler, interceptors);
    }
}
