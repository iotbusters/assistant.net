using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    private readonly string name;
    private readonly IServiceProvider provider;
    private readonly IOptionsMonitor<MessagingClientOptions> optionsMonitor;

    public MessagingClient(string name, IServiceProvider provider)
    {
        this.name = name;
        this.provider = provider;
        this.optionsMonitor = provider.GetRequiredService<IOptionsMonitor<MessagingClientOptions>>();
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public Task<object> RequestObject(object message, CancellationToken token)
    {
        var client = CreateInterceptingHandler(message.GetType());
        return client.Request(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    public Task PublishObject(object message, CancellationToken token)
    {
        var client = CreateInterceptingHandler(message.GetType());
        return client.Publish(message, token);
    }

    /// <exception cref="MessageNotRegisteredException"/>
    private InterceptingMessageHandler CreateInterceptingHandler(Type messageType)
    {
        var options = optionsMonitor.Get(name);
        if(!options.Handlers.TryGetValue(messageType, out var definition))
            throw new MessageNotRegisteredException(messageType);

        var handler = definition.Create(provider);

        var interceptors = options.Interceptors
            .Where(x => x.MessageType.IsAssignableFrom(messageType))
            .Select(x => x.Create(provider));

        return new InterceptingMessageHandler(handler, interceptors);
    }
}