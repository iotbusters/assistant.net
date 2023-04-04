using Assistant.Net.Serialization.Abstractions;
using ProtoBuf;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal;

internal class DefaultProtoSerializer<TValue> : ISerializer<TValue>
{
    public Task Serialize(Stream stream, TValue value, CancellationToken token = default)
    {
        Serializer.Serialize(stream, value);
        return Task.CompletedTask;
    }

    public Task<TValue> Deserialize(Stream stream, CancellationToken token = default)
    {
        var value = Serializer.Deserialize<TValue>(stream);
        return Task.FromResult(value);
    }
}
