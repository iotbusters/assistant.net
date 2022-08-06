using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures;

public class MessagingClientFixture : IDisposable
{
    private readonly TestConfigureOptionsSource remoteSource;
    private readonly TestConfigureOptionsSource clientSource;
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public MessagingClientFixture(
        TestConfigureOptionsSource remoteSource,
        TestConfigureOptionsSource clientSource,
        ServiceProvider provider,
        IHost host)
    {
        this.remoteSource = remoteSource;
        this.clientSource = clientSource;
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public void ReplaceHandlers(params object[] handlerInstances)
    {
        remoteSource.Configurations.Add(o =>
        {
            o.Handlers.Clear();
            foreach (var handlerInstance in handlerInstances)
                o.AddHandler(handlerInstance);
        });
        clientSource.Configurations.Add(o =>
        {
            o.Handlers.Clear();
            foreach (var handlerInstance in handlerInstances)
            {
                var handlerType = handlerInstance.GetType();
                var messageType = handlerType.GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(handlerInstances));
                o.AddGeneric(messageType);
            }
        });
        remoteSource.Reload();
        clientSource.Reload();

        host.Services.GetRequiredService<MessageAcceptanceService>().Register(TimeSpan.FromSeconds(1), default).Wait();
    }

    public virtual void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }
}
