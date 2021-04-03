using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Tests.TestObjects;

namespace Assistant.Net.Messaging.Tests.Internal
{
    public class RemoteCommandHandlingServiceTests
    {
        [Test]
        public async Task Test()
        {
            try
            {
            using var server = new TestServer(new WebHostBuilder()
            .UseKestrel(o => o.ListenAnyIP(5000))
            .Configure(b => b
                .UsePathBase("/api")
                //.UseRemoteCommandHandling()
                .UseRouting()
                .UseEndpoints(b=>b.MapRemoteCommandHandling()))//todo
            .ConfigureAppConfiguration(b => b
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables())
            .ConfigureLogging(b => b.AddConsole())
            .ConfigureServices(s => s
                // .AddAuthentication()
                // .AddAuthorization()
                .AddRouting()
                .AddJsonSerializerOptions()
                .AddCommandClient(b=>b.Handlers.AddLocal<TestCommandHandler1>())
                .AddSystemLifetime(p => p // todo
                    .GetRequiredService<IHostApplicationLifetime>().ApplicationStopping))
            );
            using var client = server.CreateClient();
            var response = await client.PostAsync("/api/command/TestCommand1", JsonContent.Create(new TestCommand1(null)));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("{}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
        }
    }
}