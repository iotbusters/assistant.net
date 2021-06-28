using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Json.Tests.Mocks
{
    public class TestClassSerializer : ISerializer<TestClass>
    {
        private readonly List<object> serializeRequests = new();
        private readonly List<byte[]> deserializeRequests = new();

        public Task Serialize(Stream stream, TestClass value)
        {
            serializeRequests.Add(value);
            return Task.CompletedTask;
        }

        public Task<TestClass> Deserialize(Stream stream)
        {
            var memory = new MemoryStream();
            stream.CopyTo(stream);
            deserializeRequests.Add(memory.ToArray());
            return Task.FromResult(new TestClass(DateTime.UtcNow));
        }

        public IEnumerable<object> SerializeRequests => serializeRequests;
        public IEnumerable<byte[]> DeserializeRequests => deserializeRequests;

    }
}