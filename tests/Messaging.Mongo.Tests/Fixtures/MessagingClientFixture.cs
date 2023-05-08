using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures;

public sealed class MessagingClientFixture : IDisposable
{
    private readonly string serverName;
    private readonly TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource;
    private readonly ServiceProvider provider;
    private readonly IHost host;

    public MessagingClientFixture(
        string serverName,
        TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource,
        TestConfigureOptionsSource<MessagingClientOptions> remoteSource,
        TestConfigureOptionsSource<MessagingClientOptions> clientSource,
        ServiceProvider provider,
        IHost host)
    {
        this.serverName = serverName;
        this.genericServerSource = genericServerSource;
        this.remoteSource = remoteSource;
        this.clientSource = clientSource;
        this.provider = provider;
        this.host = host;
    }

    public IMessagingClient Client => provider.GetRequiredService<IMessagingClient>();

    public void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }

    public async Task ReplaceHandlers(params object[] handlerInstances)
    {
        var @event = new ManualResetEventSlim();
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
            @event.Set();
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

        var timeout = TimeSpan.FromSeconds(1);
        if (!@event.Wait(timeout))
            throw new InvalidOperationException($"Options weren't updated in {timeout}.");

        var availabilityService = host.Services.GetRequiredService<IServerAvailabilityService>();
        var storage = host.Services.GetRequiredService<IStorage<string, HostsAvailabilityModel>>();

        if (await storage.TryGet(HostsAvailabilityModel.Key) is Some<HostsAvailabilityModel>(var model) && model.Registrations.Any())
        {
            var now = DateTimeOffset.UtcNow;
            var expired = model.Registrations.Max(x => x.Expired);
            var timeToLive = expired - now;

            if (now < expired)
            {
                await availabilityService.Register(serverName, timeToLive, token: default); // ensure updated.
                await Task.Delay(timeToLive); // wait until client cache has expired.
            }
        }
        else
        {
            var timeToLive = TimeSpan.FromSeconds(1);
            await availabilityService.Register(serverName, timeToLive, token: default); // ensure created.
        }
    }

    public void InactivateHost()
    {
        var service = host.Services.GetService<TestServerActivityService>()
                      ?? throw new InvalidOperationException("Server activity wasn't mocked.");
        service.DelayTime = Timeout.InfiniteTimeSpan;
    }

    public void UnregisterHost()
    {
        var strategy = provider.GetService<TestHostSelectionStrategy>()
                       ?? throw new InvalidOperationException("Server availability wasn't mocked.");
        strategy.InstanceGetter = _ => null;
    }
}
