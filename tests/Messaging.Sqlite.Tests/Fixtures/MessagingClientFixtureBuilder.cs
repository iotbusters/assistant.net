using Assistant.Net.Messaging.Abstractions;
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
    private readonly TestConfigureOptionsSource clientSource = new();
    private readonly TestConfigureOptionsSource remoteSource = new();

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
                Interval = TimeSpan.FromSeconds(0.05), MaxAttemptNumber = 15
            })
            .BindOptions(clientSource);
        RemoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
                .AddGenericMessageHandling()
                .ConfigureGenericMessagingClient(o => o
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>()
                    .ClearTransientExceptions())
                .ConfigureGenericHandlingServerOptions(o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.05);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.01);
                })
                .BindOptions(GenericOptionsNames.DefaultName, remoteSource));
    }

    public IServiceCollection Services { get; init; }
    public IHostBuilder RemoteHostBuilder { get; init; }

    public MessagingClientFixtureBuilder UseSqliteProvider(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString).UseSqliteProvider());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseSqlite(connectionString)));
        return this;
    }

    public MessagingClientFixtureBuilder UseSqliteSingleProvider(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString).UseSqliteSingleProvider());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseSqlite(connectionString)));
        return this;
    }

    public MessagingClientFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        clientSource.Configurations.Add(o =>
        {
            foreach (var messageType in messageTypes)
                o.AddGeneric(messageType);
        });
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        return this;
    }

    public MessagingClientFixtureBuilder AddSingleProviderHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                          ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

        clientSource.Configurations.Add(o => o.AddSingle(messageType));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        return this;
    }

    public MessagingClientFixtureBuilder AddMessageRegistrationOnly<TMessage>()
        where TMessage : IAbstractMessage
    {
        clientSource.Configurations.Add(o => o.AddGeneric(typeof(TMessage)));
        return this;
    }

    public MessagingClientFixture Create()
    {
        var provider = Services.BuildServiceProvider();
        var host = RemoteHostBuilder.Start();
        return new(remoteSource, clientSource, provider, host);
    }
}
