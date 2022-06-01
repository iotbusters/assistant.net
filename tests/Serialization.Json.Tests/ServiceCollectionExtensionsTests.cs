using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfTypeEncoder_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<ITypeEncoder>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializerFactory_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<ISerializerFactory>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject_configuredWithAddJsonType()
    {
        var provider = new ServiceCollection()
            .AddSerializer(b => b.AddJsonType<object>())
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject_configuredWithAddJsonTypeAny()
    {
        var provider = new ServiceCollection()
            .AddSerializer(b => b.AddJsonTypeAny())
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_throwsException_notConfigured()
    {
        var provider = new ServiceCollection()
            .AddSerializer(delegate { })
            .BuildServiceProvider();

        provider.Invoking(x => x.GetService<ISerializer<object>>())
            .Should().Throw<SerializerTypeNotRegisteredException>();
    }

    [Test]
    public void GetServiceOfSerializerOptions_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(delegate { })
            .BuildServiceProvider();

        provider.GetService<IOptions<SerializerOptions>>()
            .Should().NotBeNull();
    }
}
