using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace Assistant.Net.Messaging.Web.Server.Tests.Fixtures
{
    public class MessageClientFixtureBuilder
    {
        public MessageClientFixtureBuilder()
        {
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebMessageHandler())
                .ConfigureServices(s => s.AddRemoteWebMessageHandler(b => b.ClearInterceptors())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessageClientFixtureBuilder AddRemote<THandler>() where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMessageClient(b => b.AddLocal<THandler>()));
            return this;
        }

        public MessagingClientFixture Create()
        {
            var host = RemoteHostBuilder.Start();
            var provider = new ServiceCollection()
                .AddSingleton(host)// to dispose all at once
                .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
                .AddJsonSerialization()
                .BuildServiceProvider();
            return new(provider);
        }
    }
}