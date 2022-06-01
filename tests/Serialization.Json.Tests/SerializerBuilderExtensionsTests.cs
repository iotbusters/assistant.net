using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Json.Tests;

public class SerializerBuilderExtensionsTests
{
    [Test]
    public void AddJsonTypeAny_registersSerializers()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonTypeAny();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(IJsonSerializer), ImplementationType = new {Name = "DefaultJsonSerializer"}});
        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Should().BeEmpty();
        options?.AnyTypeRegistration.Should().NotBeNull();
    }

    [Test]
    public void AddJsonType_registersSerializers()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonType(typeof(object));

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(IJsonSerializer), ImplementationType = new {Name = "DefaultJsonSerializer"}});
        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Keys.Should().BeEquivalentTo(new[] {typeof(object)});
        options?.AnyTypeRegistration.Should().BeNull();
    }

    [Test]
    public void AddJsonTypeOfType_registersSerializersAndSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonType<object>();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(IJsonSerializer), ImplementationType = new {Name = "DefaultJsonSerializer"}});
        var options = services.BuildServiceProvider().GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("");
        options?.Registrations.Keys.Should().BeEquivalentTo(new[] {typeof(object)});
        options?.AnyTypeRegistration.Should().BeNull();
    }
        
    [Test]
    public void AddJsonConverter_registersJsonSerializerOptionsAndJsonSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonConverter<JsonStringEnumConverter>();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(JsonStringEnumConverter), ImplementationType = typeof(JsonStringEnumConverter)});
        services.BuildServiceProvider().GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get("")
            .Should().BeEquivalentTo(new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            });
    }

    [Test]
    public void AddJsonTypeOfType_registersSerializersAndEmptySerializerOptions_anotherOptionsName()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonType<object>();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(IJsonSerializer), ImplementationType = new {Name = "DefaultJsonSerializer"}});
        services.BuildServiceProvider()
            .GetService<IOptionsSnapshot<SerializerOptions>>()?.Get("another")
            .Should().BeEquivalentTo(new SerializerOptions());
    }
        
    [Test]
    public void AddJsonConverter_registersJsonSerializerOptionsAndEmptyJsonSerializerOptions_anotherOptionsName()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonConverter<JsonStringEnumConverter>();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(JsonStringEnumConverter), ImplementationType = typeof(JsonStringEnumConverter)});
        services.BuildServiceProvider()
            .GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get("another")
            .Should().BeEquivalentTo(new JsonSerializerOptions());
    }
}
