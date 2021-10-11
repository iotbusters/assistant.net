using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;

namespace Assistant.Net.Messaging.Web.Server.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GetServiceOfIMessageClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteWebMessageHandler()
                .ConfigureMessagingClient(b => b
                    .AddLocal<TestFailMessageHandler>()) // to have at least one handler configured
                .BuildServiceProvider();

            provider.GetService<IMessagingClient>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfMiddleware_resolvesObjects()
        {
            var services = new ServiceCollection()
                .AddRemoteWebMessageHandler()
                .ConfigureMessagingClient(b => b.AddLocal<TestFailMessageHandler>()); // to have at least one handler configured
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
            {
                Request = {Method = HttpMethods.Get, Path = "/", Headers = {[HeaderNames.CorrelationId] = "1"}}
            };

            var middlewareTypes = services.Select(x => x.ServiceType).Where(x => x.Name.EndsWith("Middleware")).ToArray();
            foreach (var type in middlewareTypes)
                provider.GetRequiredService(type);
        }
    }
}
