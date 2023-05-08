using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Sqlite.Tests.Mocks;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests.Fixtures;

public class MessagingClientFixtureBuilder
{
    private const string ApplicationName = "test";

    private readonly string serverName;
    private readonly TestConfigureOptionsSource<GenericHandlingServerOptions> genericServerSource = new();
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource = new();
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource = new();

    public MessagingClientFixtureBuilder(string serverName = "")
    {
        this.serverName = serverName;
        Services = new ServiceCollection()
            .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
            .AddLogging(b => b.AddYamlConsole())
            .AddMessagingClient(b => b
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>()
                .RemoveInterceptor<TimeoutInterceptor>()
                .ClearTransientExceptions())
            .ConfigureGenericHandlerProxyOptions(o => o.Poll(new ConstantBackoff
            {
                Interval = TimeSpan.FromSeconds(0.05), MaxAttemptNumber = 10
            }))
            .BindOptions(clientSource);
        RemoteHostBuilder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((ctx, _) => ctx.HostingEnvironment.ApplicationName = ApplicationName)
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
                .AddLogging(b => b.AddYamlConsole())
                .AddGenericMessageHandling(serverName)
                .ConfigureMessagingClient(serverName, b => b
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>()
                    .ClearTransientExceptions())
                .ConfigureGenericHandlingServerOptions(serverName, o =>
                {
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.05);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.01);
                })
                .Configure<HealthCheckPublisherOptions>(o => o.Period = TimeSpan.FromSeconds(1))
                .BindOptions(serverName, remoteSource)
                .BindOptions(serverName, genericServerSource));
    }

    public IServiceCollection Services { get; init; }
    public IHostBuilder RemoteHostBuilder { get; init; }

    public MessagingClientFixtureBuilder UseSqlite(string connectionString)
    {
        Services.ConfigureMessagingClient(b => b
            .UseSqlite(connectionString)
            .UseGenericSingleHandler())
            .ConfigureGenericHandlerProxyOptions(o => o
                .Poll(new ConstantBackoff { MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(0.1) }));
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureGenericMessageHandling(serverName, b => b
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

    public MessagingClientFixtureBuilder UseBackoffHandler<THandler>(THandler? handler = null) where THandler : class, IAbstractHandler
    {
        genericServerSource.Configurations.Add(o => o.BackoffHandler(true));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.UseBackoffHandler(handler);
            else
                o.UseBackoffHandler((p, _) => p.Create<THandler>());
        });
        clientSource.Configurations.Add(o => o.UseGenericBackoffHandler());
        return this;
    }

    public MessagingClientFixtureBuilder AddMessageRegistrationOnly<TMessage>()
        where TMessage : IAbstractMessage
    {
        clientSource.Configurations.Add(o => o.AddSingle<TMessage>());
        return this;
    }

    public MessagingClientFixtureBuilder AddMessageRegistrationOnly(Type messageType)
    {
        clientSource.Configurations.Add(o => o.AddSingle(messageType));
        return this;
    }

    public MessagingClientFixtureBuilder MockServerActivity()
    {
        RemoteHostBuilder.ConfigureServices(s => s
            .TryAddSingleton<TestServerActivityService>()
            .ReplaceSingleton<IServerActivityService>(p => p.GetRequiredService<TestServerActivityService>()));
        return this;
    }

    public MessagingClientFixtureBuilder MockServerAvailability(Func<Type, string?>? getter = null)
    {
        getter ??= _ => InstanceName.Create(ApplicationName, serverName);
        Services
            .TryAddSingleton(_ => new TestHostSelectionStrategy { InstanceGetter = getter })
            .ConfigureGenericHandlerProxyOptions(o => o
                .UseHostSelectionStrategy(p => p.GetRequiredService<TestHostSelectionStrategy>()));
        RemoteHostBuilder.ConfigureServices(s => s.ReplaceSingleton<IServerAvailabilityService, TestServerAvailabilityService>());
        return this;
    }

    public async Task<MessagingClientFixture> Create()
    {
        var provider = Services.BuildServiceProvider();
        var host = await RemoteHostBuilder.StartAsync();

        var activityService = host.Services.GetRequiredService<IServerActivityService>();
        await activityService.DelayInactive(token: default);
        var availabilityService = host.Services.GetRequiredService<IServerAvailabilityService>();
        await availabilityService.Register(serverName, timeToLive: TimeSpan.FromSeconds(1), token: default);

        return new(serverName, genericServerSource, remoteSource, clientSource, provider, host);
    }
}
