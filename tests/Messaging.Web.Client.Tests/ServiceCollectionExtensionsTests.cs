using System;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddRemoteCommandHandlingClient_registersServiceDescriptors()
        {
            var services = new ServiceCollection()
                .AddRemoteWebCommandClient(opt => { })
                .Services;

            services.Should().ContainEquivalentOf(new
            {
                ServiceType = typeof(IRemoteCommandClient),
                ImplementationType = (Type?) null
                //ImplementationFactory: Assistant.Net.Messaging.RemoteWebCommandClient <AddTypedClientCore>b__0(System.IServiceProvider)
            });
        }

        [Test]
        public void GetServiceOfRemoteCommandHandlingClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteWebCommandClient(opt => opt.BaseAddress = new Uri("http://localhost")).Services
                .BuildServiceProvider();

            provider.GetService<IRemoteCommandClient>()
                .Should().NotBeNull();
        }
    }
}