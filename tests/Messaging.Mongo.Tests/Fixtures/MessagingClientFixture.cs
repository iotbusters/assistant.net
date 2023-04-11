using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures;

public sealed class MessagingClientFixture : IDisposable
{
    private readonly TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource;
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public MessagingClientFixture(
        TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource,
        TestConfigureOptionsSource<MessagingClientOptions> remoteSource,
        TestConfigureOptionsSource<MessagingClientOptions> clientSource,
        ServiceProvider provider,
        IHost host)
    {
        this.genericServerSource = genericServerSource;
        this.remoteSource = remoteSource;
        this.clientSource = clientSource;
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public void ReplaceHandlers(params object[] handlerInstances)
    {
        genericServerSource.Configurations.Add(o =>
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
        genericServerSource.Reload();
        remoteSource.Reload();
        clientSource.Reload();
        Thread.Sleep(1);

        var service = host.Services.GetRequiredService<ServerAvailabilityService>();
        service.Register(TimeSpan.FromSeconds(1), default).Wait();
    }

    public TService GetServerService<TService>() where TService : class => host.Services.GetRequiredService<TService>();

    public void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }
}
