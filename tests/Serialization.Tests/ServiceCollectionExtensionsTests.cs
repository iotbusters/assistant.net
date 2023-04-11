using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Options;
using Assistant.Net.Serialization.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfTypeEncoder_resolvesObject()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<ITypeEncoder>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializerFactory_resolvesObject()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<ISerializerFactory>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject_configuredWithAddType()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(b => b.UseFormat((_, _) => new TestSerializer<TestClass>()).AddType<TestClass>())
            .BuildServiceProvider();

        provider.GetService<ISerializer<TestClass>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject_configuredWithAllowAnyType()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(b => b.UseFormat((_, _) => new TestSerializer<TestClass>()).AllowAnyType())
            .BuildServiceProvider();

        provider.GetService<ISerializer<TestClass>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_throwsException_notConfigured()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(b => b.UseFormat((_, _) => new TestSerializer<TestClass>()).AddType<TestClass>())
            .BuildServiceProvider();

        provider.Invoking(x => x.GetService<ISerializer<object>>())
            .Should().Throw<SerializingTypeNotRegisteredException>();
    }

    [Test]
    public void GetServiceOfSerializerOptions_resolvesObject()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(delegate { })
            .BuildServiceProvider();

        provider.GetService<IOptions<SerializerOptions>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_throwsException_formatNotConfigured()
    {
        using var provider = new ServiceCollection()
            .AddSerializer(b => b.AddType<TestClass>())
            .BuildServiceProvider();

        provider
            .Invoking(p => p.GetRequiredService<ISerializer<TestClass>>())
            .Should().Throw<SerializerNotRegisteredException>();
    }
}
