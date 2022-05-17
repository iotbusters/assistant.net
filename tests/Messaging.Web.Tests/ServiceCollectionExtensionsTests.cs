using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfMessageExceptionJsonConverter_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddExceptionJsonSerialization()
            .BuildServiceProvider();

        provider.GetService<MessageExceptionJsonConverter>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddExceptionJsonSerialization()
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }
}
