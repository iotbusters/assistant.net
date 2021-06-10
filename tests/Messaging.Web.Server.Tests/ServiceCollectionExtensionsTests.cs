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
        public void GetServiceOfICommandClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteWebCommandHandler(b => b
                    .AddLocal<TestFailCommandHandler>()) // to have at least one handler configured
                .BuildServiceProvider();

            provider.GetService<ICommandClient>()
                .Should().NotBeNull();
        }
    }
}