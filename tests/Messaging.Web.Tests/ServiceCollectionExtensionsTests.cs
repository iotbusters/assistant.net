using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void GetServiceOfCommandExceptionJsonConverter_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddJsonSerialization()
                .BuildServiceProvider();

            provider.GetService<CommandExceptionJsonConverter>()
                .Should().NotBeNull();
        }

        [Test]
        public void GetServiceOfSerializer_resolvesObject()
        {
            var provider = new ServiceCollection()
                .AddJsonSerialization()
                .BuildServiceProvider();

            provider.GetService<ISerializer<object>>()
                .Should().NotBeNull();
        }
    }
}