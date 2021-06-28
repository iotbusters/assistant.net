using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests.Internal
{
    public class DefaultJsonSerializerTests
    {
        [Test]
        public async Task SerializeAndDeserialize()
        {
            var serializer = new ServiceCollection()
                .AddSerializer(b => b.AddJsonType<object>())
                .BuildServiceProvider()
                .GetRequiredService<IJsonSerializer>();

            var stream = new MemoryStream();
            var value = new TestClass(DateTime.UtcNow);

            await serializer.Serialize(stream, value);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream, typeof(TestClass));

            result.Should().Be(value);
        }

    }
}