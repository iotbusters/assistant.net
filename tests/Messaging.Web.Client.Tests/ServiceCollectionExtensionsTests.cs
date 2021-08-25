using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Assistant.Net.Messaging.Web.Client.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GetServiceOfRemoteMessageHandlingClient_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddRemoteWebMessagingClient(opt => opt.BaseAddress = new Uri("http://localhost")).Services
                .BuildServiceProvider();

            provider.GetService<IRemoteMessagingClient>()
                .Should().NotBeNull();
        }
    }
}