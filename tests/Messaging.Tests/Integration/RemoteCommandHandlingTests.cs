using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;

namespace Assistant.Net.Messaging.Tests.Integration
{
    public class RemoteCommandHandlingTests
    {
        [Test]
        public async Task Test()
        {
            var host = await new HostBuilder()
                .ConfigureWebHost(wb => wb
                    //.UseKestrel(o => o.ListenAnyIP(5000))
                    .UseTestServer()
                    .Configure(b => b
                        .UsePathBase("/api")
                        //.UseRouting()
                        //.UseEndpoints(b => b.MapRemoteCommandHandling())
                        // todo
                        .UseRemoteCommandHandling()
                        )
                    .ConfigureServices(s => s
                        // todo
                        // .AddAuthentication()
                        // .AddAuthorization()
                        //.AddRouting()
                        .AddSystemServicesHosted()
                        .AddRemoteCommandHandlingServer(b => b
                            .Handlers.AddLocal<TestCommandHandler1>()))
                    )
                    .ConfigureAppConfiguration(b => b
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddEnvironmentVariables())
                    .ConfigureLogging(b => b.AddConsole())
                    .StartAsync();
            using var testHandler = host.GetTestServer().CreateHandler();
            // var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/command")
            // {
            //     Headers = { { "command-name", "TestCommand1" } },
            //     Content = JsonContent.Create(new TestCommand1(null))
            // });
            // response.StatusCode.Should().Be(HttpStatusCode.OK);
            // var content = await response.Content.ReadAsStringAsync();
            // content.Should().Be("{\"fail\":false}");
            var services = new ServiceCollection()
            .AddRemoteCommandHandlingClient(o => o.BaseAddress = new Uri("http://localhost/api/command"))
            .AddCommandClient(o => o.Handlers.AddRemote<TestCommand1>())
            .Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerFactory>(p => new TestHttpMessageHandlerFactory(testHandler)));
            var provider = services.BuildServiceProvider();
            var commandClient = provider.GetRequiredService<ICommandClient>();
            var response = await commandClient.Send(new TestCommand1(null));
            response.Should().Be(new TestResponse(false));
        }
    }
}