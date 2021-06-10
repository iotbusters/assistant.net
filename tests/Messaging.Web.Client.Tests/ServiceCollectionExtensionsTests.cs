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