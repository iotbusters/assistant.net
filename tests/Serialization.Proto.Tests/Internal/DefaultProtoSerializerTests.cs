using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Json.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Proto.Tests.Internal;

public class DefaultProtoSerializerTests
{
    [Test]
    public async Task SerializeAndDeserialize()
    {
        await using var provider = new ServiceCollection()
            .AddSerializer(b => b.UseProto().AddType<TestClass>())
            .BuildServiceProvider();
        var serializer = provider.GetRequiredService<ISerializer<TestClass>>();

        var stream = new MemoryStream();
        var value = new TestClass(DateTime.UtcNow);
        await serializer.Serialize(stream, value);
        stream.Position = 0;
        var result = await serializer.Deserialize(stream);

        result.Should().Be(value);
    }
}
