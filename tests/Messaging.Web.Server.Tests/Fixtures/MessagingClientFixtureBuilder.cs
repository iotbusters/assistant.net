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
                    .AddWebMessageHandling(b => b
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<DeferredCachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>()
                        .RemoveInterceptor<TimeoutInterceptor>())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessagingClientFixtureBuilder ClearInterceptors()
        {
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureWebMessageHandling(b => b.ClearInterceptors()));
            return this;
        }


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
