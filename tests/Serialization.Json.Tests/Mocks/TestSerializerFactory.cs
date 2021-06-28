using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Json.Tests.Mocks
{
    public class TestSerializerFactory : ISerializerFactory
    {
        private readonly List<object> serializeRequests = new();
        private readonly List<byte[]> deserializeRequests = new();

        public ISerializer<object> Create(Type serializingType)
        {
            return new TestSerializer(
                (_, o) =>
                {
                    serializeRequests.Add(o);
                    return Task.CompletedTask;
                },
                s =>
                {
                    var stream = new MemoryStream();
                    s.CopyTo(stream);
                    deserializeRequests.Add(stream.ToArray());
                    return Task.FromResult(new object());
                });
        }

        public IEnumerable<object> SerializeRequests => serializeRequests;
        public IEnumerable<byte[]>DeserializeRequests => deserializeRequests;

        private class TestSerializer : ISerializer<object>
        {
            private readonly Func<Stream, object, Task> serialize;
            private readonly Func<Stream, Task<object>> deserialize;

            public TestSerializer(Func<Stream, object, Task> serialize, Func<Stream, Task<object>> deserialize)
            {
                this.serialize = serialize;
                this.deserialize = deserialize;
            }

            public Task Serialize(Stream stream, object value) => serialize(stream, value);

            public Task<object> Deserialize(Stream stream) => deserialize(stream);
        }
    }
}