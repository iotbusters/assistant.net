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
        public void AddRemoteCommandHandlingClient_registersServiceDescriptors()
        {
            var services = new ServiceCollection()
                .AddRemoteCommandHandlingServer(opt => { });

            services.Should().ContainEquivalentOf(new
            {
                ServiceType = typeof(ICommandClient),
                ImplementationType = new { Name = "CommandClient" },
            });
        }

        [Test]
        public void GetServiceOfRemoteCommandHandlingClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteCommandHandlingServer(opt => opt
                    .Handlers.AddLocal<TestFailCommandHandler>()) // to have at least one handler configured
                .BuildServiceProvider();

            provider.GetService<ICommandClient>()
                .Should().NotBeNull();
        }
    }
}