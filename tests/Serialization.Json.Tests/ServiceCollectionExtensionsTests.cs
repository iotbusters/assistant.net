using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfExceptionJsonConverter_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<ExceptionJsonConverter<Exception>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfAdvancedJsonConverter_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<AdvancedJsonConverter<TestClass>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfEnumerableJsonConverter_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => {})
            .BuildServiceProvider();

        provider.GetService<EnumerableJsonConverter<TestClass>>()
            .Should().NotBeNull();
    }

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
    public void GetServiceOfSerializer_resolvesObject_configured()
    {
        var provider = new ServiceCollection()
            .AddSerializer(b => b.AddJsonType<object>())
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject_notConfigured()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => { })
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfJsonSerializerOptions_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddSerializer(_ => { })
            .BuildServiceProvider();

        provider.GetService<IOptions<JsonSerializerOptions>>()
            .Should().NotBeNull();
    }
}
