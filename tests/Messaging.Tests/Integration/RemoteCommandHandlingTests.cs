using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging.Tests.Integration
{
    public class RemoteCommandHandlingTests
    {
        [Test]
        public async Task Test()
        {
            var host = await new HostBuilder()
                .ConfigureWebHost(wb => wb
                    .UseTestServer()
                    .Configure(b => b.UseRemoteCommandHandling())
                    .ConfigureServices(s => s
                        .AddSystemServicesHosted()
                        .AddRemoteCommandHandlingServer(b => b.Handlers
                            .AddLocal<TestCommandHandler1>())))
                    .StartAsync();

            var commandClient = new ServiceCollection()
                .AddRemoteCommandHandlingClient(o => o.BaseAddress = new Uri("http://localhost/command"))
                .AddCommandClient(o => o.Handlers.AddRemote<TestCommand1>())
                .AddHttpClientRedirect<RemoteCommandHandlingClient>(host)
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();

            var response = await commandClient.Send(new TestCommand1(null));

            response.Should().Be(new TestResponse(false));
        }
    }
}