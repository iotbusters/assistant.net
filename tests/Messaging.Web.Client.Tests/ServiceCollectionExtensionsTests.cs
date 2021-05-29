using System;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Web.Client.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddRemoteCommandHandlingClient_registersServiceDescriptors()
        {
            var services = new ServiceCollection()
                .AddRemoteCommandHandlingClient(opt => {});

            services.Should().ContainEquivalentOf(new
            {
                ServiceType = typeof(RemoteCommandHandlingClient),
                ImplementationType = (Type?) null,
                //ImplementationFactory: Assistant.Net.Messaging.RemoteCommandHandlingClient <AddTypedClientCore>b__0(System.IServiceProvider)
            });
        }

        [Test]
        public void GetServiceOfRemoteCommandHandlingClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteCommandHandlingClient(opt => opt.BaseAddress = new Uri("http://localhost"))
                .BuildServiceProvider();

            provider.GetService<RemoteCommandHandlingClient>()
                .Should().NotBeNull();
        }
    }
}