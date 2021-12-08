using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

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
                    .AddWebMessageHandling(_ => { })
                    .ConfigureMessagingClient(WebOptionsNames.DefaultName, b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessagingClientFixtureBuilder AddWebHandler<THandler>() where THandler : class
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureWebMessageHandling(b => b.AddHandler<THandler>())
                .ConfigureMessagingClient(WebOptionsNames.DefaultName, b => b.AddHandler(typeof(THandler))));
            return this;
        }
        
        public MessagingClientFixture Create()
        {
            var host = RemoteHostBuilder.Start();
            return new(host);
        }
    }
}
