using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Linq;

namespace Assistant.Net.Messaging.Web.Tests.Fixtures
{
    public class MessagingClientFixtureBuilder
    {
        private readonly TestConfigureOptionsSource remoteSource = new();
        private readonly TestConfigureOptionsSource clientSource = new();

        public MessagingClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .UseWeb(hcb => hcb.ConfigureHttpClient(hc =>
                    {
                        hc.BaseAddress = new Uri("http://localhost/messages");
                        // debugging purpose only.
                        if (Debugger.IsAttached)
                            hc.Timeout = TimeSpan.FromSeconds(300);
                    }))
                    .TimeoutIn(TimeSpan.FromSeconds(0.5))
                    .ClearInterceptors())
                .AddOptions<MessagingClientOptions>()
                .Bind(clientSource)
                .Services;
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebMessageHandler()))
                .ConfigureServices(s => s
                    .AddWebMessageHandling(_ => { })
                    .ConfigureMessagingClient(b => b.ClearInterceptors())
                    .AddOptions<MessagingClientOptions>(WebOptionsNames.DefaultName)
                    .Bind(remoteSource));
        }

        public MessagingClientFixtureBuilder ClearInterceptors()
        {
            Services.ConfigureMessagingClient(b => b.ClearInterceptors());
            return this;
        }

        public MessagingClientFixtureBuilder AddWebHandler<THandler>(THandler? handler = null) where THandler : class
        {
            remoteSource.Configurations.Add(o =>
            {
                if (handler != null)
                    o.AddLocalHandler(handler);
                else
                    o.AddLocalHandler(typeof(THandler));
            });
            clientSource.Configurations.Add(o =>
            {
                var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                                  ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));
                o.AddWeb(messageType);
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
            Services.ConfigureMessagingClient(b => b.AddWeb(messageType));
            return this;
        }

        public MessagingClientFixture Create()
        {
            var host = RemoteHostBuilder.Start();
            var provider = Services
                .AddHttpClientRedirect<IWebMessageHandlerClient>(_ => host)
                .BuildServiceProvider();
            return new(remoteSource, clientSource, provider, host);
        }

        private IServiceCollection Services { get; init; }
        private IHostBuilder RemoteHostBuilder { get; init; }

    }
}
