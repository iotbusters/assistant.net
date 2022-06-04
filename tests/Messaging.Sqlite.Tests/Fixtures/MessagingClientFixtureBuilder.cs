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
    private readonly TestConfigureOptionsSource remoteSource = new();
    private readonly TestConfigureOptionsSource clientSource = new();

    public MessagingClientFixtureBuilder()
    {
        Services = new ServiceCollection()
            .AddMessagingClient(b => b
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>()
                .RemoveInterceptor<TimeoutInterceptor>())
            .ConfigureSqliteHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.05), MaxAttemptNumber = 5
            })
            .BindOptions(clientSource);
        RemoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(s => s
                .AddSqliteMessageHandling(b => b
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>())
                .ConfigureSqliteHandlingServerOptions(o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                })
                .BindOptions(SqliteOptionsNames.DefaultName, remoteSource));
    }

    public IServiceCollection Services { get; init; }
    public IHostBuilder RemoteHostBuilder { get; init; }

    public MessagingClientFixtureBuilder UseSqlite(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString)
            .UseSqliteProvider()
            .TimeoutIn(TimeSpan.FromSeconds(0.5)));
        RemoteHostBuilder.ConfigureServices(s => s
            .ConfigureSqliteMessageHandling(b => b
            .UseSqlite(connectionString)
            .TimeoutIn(TimeSpan.FromSeconds(0.5))));
        return this;
    }

    public MessagingClientFixtureBuilder UseSqliteSingleProvider(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(o => o.Connection(connectionString))
            .UseSqliteSingleProvider()
            .TimeoutIn(TimeSpan.FromSeconds(0.5)));
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureSqliteMessageHandling(b => b
            .UseSqlite(o => o.Connection(connectionString))
            .TimeoutIn(TimeSpan.FromSeconds(0.5))));
        return this;
    }

    public MessagingClientFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                          ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o => o.AddSqlite(messageType));
        return this;
    }

    public MessagingClientFixtureBuilder AddSingleProviderHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                          ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o => o.Add(messageType));
        return this;
    }

    public MessagingClientFixtureBuilder AddMessageRegistrationOnly<TMessage>()
        where TMessage : IAbstractMessage
    {
        clientSource.Configurations.Add(o => o.AddSqlite(typeof(TMessage)));
        return this;
    }

    public MessagingClientFixture Create()
    {
        var provider = Services.BuildServiceProvider();
        var host = RemoteHostBuilder.Start();
        return new(remoteSource, clientSource, provider, host);
    }
}
