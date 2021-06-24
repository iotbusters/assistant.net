using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Fixtures
{
    public class CommandClientFixtureBuilder
    {
        public CommandClientFixtureBuilder()
        {
            RemoteHostBuilder = new HostBuilder().ConfigureWebHost(wb => wb
                .UseTestServer()
                .Configure(b => b.UseRemoteWebCommandHandler())
                .ConfigureServices(s => s.AddRemoteWebCommandHandler(b => b.ClearInterceptors())));
        }

        public IHostBuilder RemoteHostBuilder { get; init; }

        public CommandClientFixtureBuilder AddRemote<THandler>() where THandler : class, IAbstractCommandHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureCommandClient(b => b.AddLocal<THandler>()));
            return this;
        }

        public CommandClientFixture Create()
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