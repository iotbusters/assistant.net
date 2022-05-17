using Assistant.Net.Serialization.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestClassSerializer : ISerializer<TestClass>
{
    public Task Serialize(Stream stream, TestClass value, CancellationToken token) => Task.CompletedTask;

    public Task<TestClass> Deserialize(Stream stream, CancellationToken token) => Task.FromResult(new TestClass(DateTime.UtcNow));
}
