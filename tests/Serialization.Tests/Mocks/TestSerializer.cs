using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Tests.Mocks;

public class TestSerializer<T> : ISerializer<T>
{
    public Task Serialize(Stream stream, T value, CancellationToken token = default) => Task.CompletedTask;

    public Task<T> Deserialize(Stream stream, CancellationToken token = default)
    {
        return Task.FromResult(default(T))!;
    }
}
