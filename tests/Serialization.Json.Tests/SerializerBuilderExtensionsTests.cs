using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;

namespace Assistant.Net.Serialization.Json.Tests
{
    public class SerializerBuilderExtensionsTests
    {
        [Test]
        public void AddJsonTypeAny_registersSerializers()
        {
            var services = new ServiceCollection();
            var builder = new SerializerBuilder(services);

            builder.AddJsonTypeAny();

            services.Should().BeEquivalentTo(
                new
                {
                    ServiceType = typeof(IJsonSerializer),
                    ImplementationType = new { Name = "DefaultJsonSerializer" }
                },
                new
                {
                    ServiceType = typeof(ISerializer<>),
                    ImplementationType = new { Name = "TypedJsonSerializer`1" }
                });
        }

        [Test]
        public void AddJsonType_registersSerializers()
        {
            var services = new ServiceCollection();
            var builder = new SerializerBuilder(services);

            builder.AddJsonType(typeof(object));

            services.Should().BeEquivalentTo(
                new
                {
                    ServiceType = typeof(IJsonSerializer),
                    ImplementationType = new {Name = "DefaultJsonSerializer"}
                },
                new
                {
                    ServiceType = typeof(ISerializer<object>),
                    ImplementationType = new {Name = "TypedJsonSerializer`1"}
                });
        }

        [Test]
        public void AddJsonTypeOfType_registersSerializers()
        {
            var services = new ServiceCollection();
            var builder = new SerializerBuilder(services);

            builder.AddJsonType<object>();

            services.Should().BeEquivalentTo(
                new
                {
                    ServiceType = typeof(IJsonSerializer),
                    ImplementationType = new {Name = "DefaultJsonSerializer"}
                },
                new
                {
                    ServiceType = typeof(ISerializer<object>),
                    ImplementationType = new {Name = "TypedJsonSerializer`1"}
                });
        }
        
        [Test]
        public void AddJsonConverter_registersJsonSerializerOptions()
        {
            var services = new ServiceCollection();
            var builder = new SerializerBuilder(services);

            builder.AddJsonConverter<JsonStringEnumConverter>();

            services.Should().ContainEquivalentOf(
                new
                {
                    ServiceType = typeof(JsonStringEnumConverter),
                    ImplementationType = typeof(JsonStringEnumConverter)
                });
            services.BuildServiceProvider().GetService<IOptions<JsonSerializerOptions>>()?.Value
                .Should().BeEquivalentTo(new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                });
        }
    }
}