using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal;

internal class TypedJsonSerializer<TValue> : ISerializer<TValue>
{
    private readonly IJsonSerializer serializer;

    public TypedJsonSerializer(IJsonSerializer serializer) =>
        this.serializer = serializer;

    public Task Serialize(Stream stream, TValue value, CancellationToken token) =>
        serializer.Serialize(stream, value!, token);

    public async Task<TValue> Deserialize(Stream stream, CancellationToken token) =>
        (TValue) await serializer.Deserialize(stream, typeof(TValue), token);
}