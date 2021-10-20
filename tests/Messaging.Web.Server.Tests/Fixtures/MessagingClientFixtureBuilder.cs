using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace Assistant.Net.Messaging.Web.Server.Tests.Fixtures
{
    public class MessagingClientFixtureBuilder
    {
        public MessagingClientFixtureBuilder()
        {
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebMessageHandler())
                .ConfigureServices(s => s
                    .AddWebMessageHandling()
                    .ConfigureMessagingClient(b => b.ClearInterceptors())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessagingClientFixtureBuilder AddWebHandler<THandler>() where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMessagingClient(b => b.AddWebHandler<THandler>()));
            return this;
        }

        public MessagingClientFixture Create()
        {
            var host = RemoteHostBuilder.Start();
            var provider = new ServiceCollection()
                .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
                .AddJsonSerialization()
                .BuildServiceProvider();
            return new(provider, host);
        }
    }
}