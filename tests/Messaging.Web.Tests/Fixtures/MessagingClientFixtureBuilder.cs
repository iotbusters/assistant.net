using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Net.Messaging.Web.Tests.Fixtures;

public class MessagingClientFixtureBuilder
{
    private readonly string serverName;
    private readonly TestConfigureOptionsSource<MessagingClientOptions> remoteSource = new();
    private readonly TestConfigureOptionsSource<MessagingClientOptions> clientSource = new();
    private readonly TestConfigureOptionsSource<WebHandlingServerOptions> webServerSource = new();

    public MessagingClientFixtureBuilder(string serverName = "")
    {
        this.serverName = serverName;
        Services = new ServiceCollection()
            .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
            .AddLogging(b => b.AddYamlConsole())
            .AddMessagingClient(b => b
                .UseWeb(c =>
                {
                    c.BaseAddress = new("http://localhost");
                    // debugging purpose only.
                    if (Debugger.IsAttached)
                        c.Timeout = TimeSpan.FromSeconds(300);
                })
                .UseWebSingleHandler()
                .RemoveInterceptor<CachingInterceptor>()
                .RemoveInterceptor<RetryingInterceptor>()
                .RemoveInterceptor<TimeoutInterceptor>())
            .BindOptions(clientSource);
        RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseWebMessageHandling(serverName)))
            .ConfigureServices(s => s
                .AddTypeEncoder(o => o.Exclude("NUnit").Exclude("Newtonsoft"))
                .AddLogging(b => b.AddYamlConsole())
                .AddWebMessageHandling(serverName)
                .ConfigureMessagingClient(serverName, b => b
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>())
                .BindOptions(serverName, remoteSource)
                .BindOptions(serverName, webServerSource));
    }

    public MessagingClientFixtureBuilder ClearInterceptors()
    {
        Services.ConfigureMessagingClient(b => b.ClearInterceptors());
        RemoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClient(serverName, b => b.ClearInterceptors()));
        return this;
    }

    public MessagingClientFixtureBuilder AddHandler<THandler>(THandler? handler = null) where THandler : class
    {
        var messageTypes = typeof(THandler).GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        if (!messageTypes.Any())
            throw new ArgumentException($"Expected message handler but provided {typeof(THandler)}.", nameof(THandler));

        webServerSource.Configurations.Add(o => o.AcceptMessages(messageTypes));
        remoteSource.Configurations.Add(o =>
        {
            if (handler != null)
                o.AddHandler(handler);
            else
                o.AddHandler(typeof(THandler));
        });
        clientSource.Configurations.Add(o =>
        {
            var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                              ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));
            o.AddSingle(messageType);
        });
        return this;
    }

    /// <summary>
    ///     Registers the message type on client only. Server doesn't know about the message!
    /// </summary>
    public MessagingClientFixtureBuilder AddWeb<TMessage>()
        where TMessage : IAbstractMessage
    {
        var messageType = typeof(TMessage);
        Services.ConfigureMessagingClient(b => b.AddSingle(messageType));
        return this;
    }

    public MessagingClientFixture Create()
    {
        var host = RemoteHostBuilder.Start();
        var provider = Services
            .AddHttpClientRedirect<IWebMessageHandlerClient>(host)
            .BuildServiceProvider();
        return new(webServerSource, remoteSource, clientSource, provider, host);
    }

    private IServiceCollection Services { get; init; }
    private IHostBuilder RemoteHostBuilder { get; init; }

}
