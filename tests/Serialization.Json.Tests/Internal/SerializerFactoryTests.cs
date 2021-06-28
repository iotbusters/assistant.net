using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests.Internal
{
    public class SerializerFactoryTests
    {
        private ISerializerFactory factory = null!;

        [SetUp]
        public void Setup() =>
            factory = new ServiceCollection()
                .AddSerializer()
                .AddSingleton<ISerializer<TestClass>, TestClassSerializer>()
                .BuildServiceProvider()
                .GetRequiredService<ISerializerFactory>();

        [Test]
        public void Create_returnsSerializer_unregistered() =>
            factory.Create(typeof(object)).Should().NotBeNull();

        [Test]
        public void Create_returnsSerializer_registered() =>
            factory.Create(typeof(TestClass)).Should().NotBeNull();

        [Test]
        public void Serialize_throws_unregistered() =>
            factory.Create(typeof(object))
                .Invoking(x => x.Serialize(new MemoryStream(), new TestClass(DateTime.UtcNow)))
                .Should().Throw<SerializerTypeNotRegisteredException>();

        [Test]
        public void Deserialize_throws_unregistered() =>
            factory.Create(typeof(object))
                .Invoking(x => x.Deserialize(new MemoryStream()))
                .Should().Throw<SerializerTypeNotRegisteredException>();
    }
}