using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures;

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
                Interval = TimeSpan.FromSeconds(0.02), MaxAttemptNumber = 10
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
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                })
                .BindOptions(GenericOptionsNames.DefaultName, remoteSource)
                .Configure<HealthCheckPublisherOptions>(o =>
                {
                    o.Period = TimeSpan.FromSeconds(1);
                    o.Timeout = TimeSpan.FromSeconds(1);
                }));
    }

    public IServiceCollection Services { get; init; }
    public IHostBuilder RemoteHostBuilder { get; init; }

    public MessagingClientFixtureBuilder UseMongoProvider(string connectionString, string database)
    {
        Services.ConfigureMessagingClient(b => b
            .UseMongo(o => o.Connection(connectionString).Database(database)).UseMongoProvider());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseMongo(o => o.Connection(connectionString).Database(database))));
        return this;
    }

    public MessagingClientFixtureBuilder UseMongoSingleProvider(string connectionString, string database)
    {
        Services.ConfigureMessagingClient(b => b
            .UseMongo(o => o.Connection(connectionString).Database(database)).UseMongoSingleProvider());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(b => b
            .UseMongo(o => o.Connection(connectionString).Database(database))));
        return this;
    }

    public MessagingClientFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                          ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

        clientSource.Configurations.Add(o => o.AddGeneric(messageType));
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

        host.Services.GetRequiredService<MessageAcceptanceService>().Register(TimeSpan.FromSeconds(1), default).Wait();

        return new(remoteSource, clientSource, provider, host);
    }
}
