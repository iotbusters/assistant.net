using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Tests;

public class SerializerBuilderExtensionsTests
{
    [Test]
    public void AllowAnyType_configuresSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AllowAnyType();

        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Should().BeEmpty();
        options?.IsAnyTypeAllowed.Should().BeTrue();
    }

    [Test]
    public void AddType_configuresSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddType(typeof(object));

        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Should().BeEquivalentTo(new[] {typeof(object)});
        options?.IsAnyTypeAllowed.Should().BeFalse();
    }

    [Test]
    public void AddTypeOfType_configuresSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddType<object>();

        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Should().BeEquivalentTo(new[] {typeof(object)});
        options?.IsAnyTypeAllowed.Should().BeFalse();
    }

    [Test]
    public void AddTypeOfType_configuresNoAnotherSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddType<object>();

        services.BuildServiceProvider()
            .GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("another")
            .Should().BeEquivalentTo(new SerializerOptions());
    }
}
