using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Sqlite.Tests.Mocks;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.Sqlite.Tests.Fixtures;

public class MessagingClientFixtureBuilder
{
    private readonly TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource = new();
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource = new();
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource = new();

    public MessagingClientFixtureBuilder()
    {
        Services = new ServiceCollection()
            .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
            .AddMessagingClient(b => b
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>()
                .RemoveInterceptor<TimeoutInterceptor>()
                .ClearTransientExceptions())
            .ConfigureGenericHandlerProxyOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.05), MaxAttemptNumber = 10
            })
            .BindOptions(clientSource);
        RemoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
                .AddGenericMessageHandling()
                .ConfigureMessagingClient(GenericOptionsNames.DefaultName, o => o
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>()
                    .ClearTransientExceptions())
                .ConfigureGenericHandlingServerOptions(o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.05);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.01);
                })
                .BindOptions(GenericOptionsNames.DefaultName, remoteSource)
                .BindOptions(genericServerSource));
    }

    public IServiceCollection Services { get; init; }
    public IHostBuilder RemoteHostBuilder { get; init; }

    public MessagingClientFixtureBuilder UseSqliteProvider(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString).UseGenericSingleHandler());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseSqlite(connectionString)));
        return this;
    }

    public MessagingClientFixtureBuilder UseSqliteSingleProvider(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString).UseGenericSingleHandler());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseSqlite(connectionString)));
        return this;
    }

    public MessagingClientFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        genericServerSource.Configurations.Add(o => o.AcceptMessages(messageTypes));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o =>
        {
            foreach (var messageType in messageTypes)
                o.AddSingle(messageType);
        });
        return this;
    }

    public MessagingClientFixtureBuilder AddAnyProviderHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        genericServerSource.Configurations.Add(o => o.AcceptMessages(messageTypes));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o => o.UseGenericBackoffHandler());
        return this;
    }

    public MessagingClientFixtureBuilder AddSingleProviderHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        genericServerSource.Configurations.Add(o => o.AcceptMessages(messageTypes));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o =>
        {
            foreach (var messageType in messageTypes)
                o.AddSingle(messageType);
        });
        return this;
    }

    public MessagingClientFixtureBuilder AddAnySingleProviderHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        genericServerSource.Configurations.Add(o => o.AcceptMessages(messageTypes));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o => o.UseGenericBackoffHandler());
        return this;
    }

    public MessagingClientFixtureBuilder AddMessageRegistrationOnly<TMessage>()
        where TMessage : IAbstractMessage
    {
        clientSource.Configurations.Add(o => o.AddSingle(typeof(TMessage)));
        return this;
    }

    public MessagingClientFixture Create()
    {
        var provider = Services.BuildServiceProvider();
        var host = RemoteHostBuilder.Start();

        host.Services.GetRequiredService<ServerAvailabilityService>().Register(TimeSpan.FromSeconds(1), default).Wait();
        host.Services.GetRequiredService<ServerActivityService>().Activate();

        return new(genericServerSource, remoteSource, clientSource, provider, host);
    }
}
