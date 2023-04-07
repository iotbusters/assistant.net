using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Converters;
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
    public void UseJson_registersFormatAndConfiguresOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.UseJson();

        services.Should().ContainEquivalentOf(new { ServiceType = typeof(ITypeEncoder) });
        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(AdvancedJsonConverterFactory), ImplementationType = typeof(AdvancedJsonConverterFactory) });
        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(ExceptionJsonConverter<>), ImplementationType = typeof(ExceptionJsonConverter<>) });
        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(EnumerableJsonConverter<>), ImplementationType = typeof(EnumerableJsonConverter<>)});
        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(AdvancedJsonConverter<>), ImplementationType = typeof(AdvancedJsonConverter<>) });
        services.BuildServiceProvider().GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get("").Converters
            .Should().Contain(x => x is AdvancedJsonConverterFactory);
        services.BuildServiceProvider().GetService<INamedOptions<SerializerOptions>>()?.Value.FormatSerializerFactory
            .Should().NotBeNull();
    }

    [Test]
    public void AddJsonConverter_registersJsonConverterAndConfiguresSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new SerializerBuilder(services, "");

        builder.AddJsonConverter<JsonStringEnumConverter>();

        services.Should().ContainEquivalentOf(
            new {ServiceType = typeof(JsonStringEnumConverter), ImplementationType = typeof(JsonStringEnumConverter)});
        services.BuildServiceProvider().GetService<IOptionsSnapshot<JsonSerializerOptions>>()?.Get("")
            .Should().BeEquivalentTo(new JsonSerializerOptions {Converters = {new JsonStringEnumConverter()}});
    }

    [Test]
    public void AddJsonConverter_registersJsonConverterButDoesNotConfigureAnotherJsonSerializerOptions()
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
