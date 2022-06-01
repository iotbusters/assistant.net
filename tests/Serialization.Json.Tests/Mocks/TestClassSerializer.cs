using Assistant.Net.Serialization.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestClassSerializer : IJsonSerializer
{
    public Task Serialize(Stream stream, object value, CancellationToken token = default) => Task.CompletedTask;

    public Task<object> Deserialize(Stream stream, Type type, CancellationToken token = default) => Task.FromResult<object>(null!);
}
