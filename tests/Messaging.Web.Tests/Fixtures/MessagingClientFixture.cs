using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests.Fixtures;

public class MessagingClientFixture : IDisposable
{
    private readonly TestConfigureOptionsSource<WebHandlingServerOptions> webServerSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource;
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public MessagingClientFixture(
        TestConfigureOptionsSource<WebHandlingServerOptions> webServerSource,
        TestConfigureOptionsSource<MessagingClientOptions> remoteSource,
        TestConfigureOptionsSource<MessagingClientOptions> clientSource,
        ServiceProvider provider,
        IHost host)
    {
        this.webServerSource = webServerSource;
        this.remoteSource = remoteSource;
        this.clientSource = clientSource;
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public async Task<object> WebRequest(IAbstractMessage message)
    {
        var client = provider.GetRequiredService<IWebMessageHandlerClient>();
        return await client.DelegateHandling(message);
    }

    public void ReplaceHandlers(params object[] handlerInstances)
    {
        webServerSource.Configurations.Add(o =>
        {
            o.MessageTypes.Clear();
            foreach (var handlerInstance in handlerInstances)
            {
                var handlerType = handlerInstance.GetType();
                var messageType = handlerType.GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(handlerInstances));
                o.AcceptMessage(messageType);
            }
        });
        remoteSource.Configurations.Add(o =>
        {
            o.HandlerFactories.Clear();
            foreach (var handlerInstance in handlerInstances)
                o.AddHandler(handlerInstance);
        });
        clientSource.Configurations.Add(o =>
        {
            o.HandlerFactories.Clear();
            foreach (var handlerInstance in handlerInstances)
            {
                var handlerType = handlerInstance.GetType();
                var messageType = handlerType.GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(handlerInstances));
                o.AddSingle(messageType);
            }
        });
        webServerSource.Reload();
        remoteSource.Reload();
        clientSource.Reload();
    }

    public virtual void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }
}
