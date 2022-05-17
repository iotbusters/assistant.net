using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Json.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;

namespace Assistant.Net.Serialization.Json.Tests.Internal;

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
            .Awaiting(x => x.SerializeObject(new MemoryStream(), new TestClass(DateTime.UtcNow)))
            .Should().ThrowAsync<SerializerTypeNotRegisteredException>();

    [Test]
    public void Deserialize_throws_unregistered() =>
        factory.Create(typeof(object))
            .Awaiting(x => x.DeserializeObject(new MemoryStream()))
            .Should().ThrowAsync<SerializerTypeNotRegisteredException>();
}
