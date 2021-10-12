using Assistant.Net.Messaging.Abstractions;
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
    public class MessageClientFixtureBuilder
    {
        public MessageClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .UseWeb(hcb => hcb
                        .ConfigureHttpClient(hc =>
                        {
                            hc.BaseAddress = new Uri("http://localhost/messages");
                            // debugging purpose only.
                            if (Debugger.IsAttached)
                                hc.Timeout = TimeSpan.FromSeconds(300);
                        }))
                    .ClearInterceptors());
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebMessageHandler()))
                .ConfigureServices(s => s
                    .AddWebMessageHandling()
                    .ConfigureMessagingClient(b => b.ClearInterceptors()));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessageClientFixtureBuilder ClearHandlers()
        {
            Services.ConfigureMessagingClient(b => b.ClearInterceptors());
            return this;
        }

        public MessageClientFixtureBuilder AddLocal<THandler>() where THandler : class, IAbstractHandler
        {
            Services.ConfigureMessagingClient(b => b.AddLocal<THandler>());
            return this;
        }

        public MessageClientFixtureBuilder AddRemote<THandler>() where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMessagingClient(b => b.AddLocal<THandler>()));

            var messageType = typeof(THandler)
                .GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMessageHandler<,>))
                ?.GetGenericArguments().First()
                ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

            Services.ConfigureMessagingClient(b => b.AddWeb(messageType));
            return this;
        }

        public MessageClientFixtureBuilder AddRemoteMessageRegistrationOnly<TMessage>()
            where TMessage : IAbstractMessage
        {
            var messageType = typeof(TMessage);
            Services.ConfigureMessagingClient(b => b.AddWeb(messageType));
            return this;
        }

        public MessageClientFixture Create()
        {
            var host = RemoteHostBuilder.Start();
            var provider = Services
                .AddHttpClientRedirect<IWebMessageHandlerClient>(_ => host)
                .BuildServiceProvider();
            return new(provider, host);
        }
    }
}
