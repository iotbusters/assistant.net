using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Json.Tests.Mocks;
using Assistant.Net.Serialization.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Proto.Tests;

public class SerializerBuilderExtensionsTests
{
    [Test]
    public void UseProto_registersFormatAndConfiguresSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.UseProto();

        services.BuildServiceProvider().GetService<INamedOptions<SerializerOptions>>()?.Value.Format
            .Should().NotBeNull();
    }

    [Test]
    public void AddTypeProtoOfType_registersSerializingType()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddTypeProto<TestClass>();

        services.BuildServiceProvider().GetService<INamedOptions<SerializerOptions>>()?.Value.Registrations
            .Should().BeEquivalentTo(new[] {typeof(TestClass)});
    }

    [Test]
    public void AddTypeProto_registersSerializingType()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddTypeProto(typeof(TestClass));

        services.BuildServiceProvider().GetService<INamedOptions<SerializerOptions>>()?.Value.Registrations
            .Should().BeEquivalentTo(new[] {typeof(TestClass)});
    }
}
