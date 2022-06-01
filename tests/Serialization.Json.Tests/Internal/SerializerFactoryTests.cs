using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Json.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Assistant.Net.Serialization.Json.Tests.Internal;

public class SerializerFactoryTests
{
    [Test]
    public void Create_throwsException_unregistered()
    {
        var factory = new ServiceCollection()
            .AddSerializer(b => b.AddJsonType<TestClass>())
            .BuildServiceProvider()
            .GetRequiredService<ISerializerFactory>();

        factory.Invoking(x => x.Create(typeof(object)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'Object' wasn't registered.");
    }

    [Test]
    public void Create_returnsSerializer_registeredWithAddJsonType()
    {
        var factory = new ServiceCollection()
            .AddSerializer(b => b.AddJsonType<TestClass>())
            .BuildServiceProvider()
            .GetRequiredService<ISerializerFactory>();
        factory.Create(typeof(TestClass)).Should().NotBeNull();
    }

    [Test]
    public void Create_returnsSerializer_registeredWithAddJsonTypeAny()
    {
        var factory = new ServiceCollection()
            .AddSerializer(b => b.AddJsonTypeAny())
            .BuildServiceProvider()
            .GetRequiredService<ISerializerFactory>();
        factory.Create(typeof(TestClass)).Should().NotBeNull();
    }

    [Test]
    public void Create_returnsSerializer_registeredAsNamedSerializers()
    {
        var provider = new ServiceCollection()
            .AddSerializer(b => b.AddJsonType<object>())
            .ConfigureSerializer("1", b => b.AddJsonType<DateTime>())
            .ConfigureSerializer("2", b => b.AddJsonType<DateTimeOffset>())
            .BuildServiceProvider();

        var factory0 = provider.CreateAsyncScope().ServiceProvider.GetRequiredService<ISerializerFactory>();
        factory0.Create(typeof(object)).Should().NotBeNull();
        factory0.Invoking(x => x.Create(typeof(DateTime)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'DateTime' wasn't registered.");
        factory0.Invoking(x => x.Create(typeof(DateTimeOffset)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'DateTimeOffset' wasn't registered.");

        var factory1 = provider.CreateAsyncScopeWithNamedOptionContext("1").ServiceProvider.GetRequiredService<ISerializerFactory>();
        factory1.Invoking(x => x.Create(typeof(object)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'Object' wasn't registered.");
        factory1.Create(typeof(DateTime)).Should().NotBeNull();
        factory1.Invoking(x => x.Create(typeof(DateTimeOffset)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'DateTimeOffset' wasn't registered.");

        var factory2 = provider.CreateAsyncScopeWithNamedOptionContext("2").ServiceProvider.GetRequiredService<ISerializerFactory>();
        factory2.Invoking(x => x.Create(typeof(object)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'Object' wasn't registered.");
        factory2.Invoking(x => x.Create(typeof(DateTime)))
            .Should().Throw<SerializerTypeNotRegisteredException>()
            .WithMessage("Type 'DateTime' wasn't registered.");
        factory2.Create(typeof(DateTimeOffset)).Should().NotBeNull();
    }

}
