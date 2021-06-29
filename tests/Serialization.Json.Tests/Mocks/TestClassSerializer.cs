using System;
using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Json.Tests.Mocks
{
    public class TestClassSerializer : ISerializer<TestClass>
    {
        public Task Serialize(Stream stream, TestClass value) => Task.CompletedTask;

        public Task<TestClass> Deserialize(Stream stream) => Task.FromResult(new TestClass(DateTime.UtcNow));
    }
}