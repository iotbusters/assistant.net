using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Server.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GetServiceOfIMessageClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteWebMessageHandler(b => b
                    .AddLocal<TestFailMessageHandler>()) // to have at least one handler configured
                .BuildServiceProvider();

            provider.GetService<IMessagingClient>()
                .Should().NotBeNull();
        }
    }
}