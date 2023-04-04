using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal;

/// <summary>
///     Serializer factory adapter default implementation.
/// </summary>
internal sealed class SerializerFactoryAdapter<T> : ISerializer<T>
{
    private readonly ISerializer<T> serializer;

    /// <summary/>
    public SerializerFactoryAdapter(ISerializerFactory factory) =>
        this.serializer = factory.Create<T>();

    /// <inheritdoc/>
    public Task Serialize(Stream stream, T value, CancellationToken token) =>
        serializer.Serialize(stream, value, token);

    /// <inheritdoc/>
    public Task<T> Deserialize(Stream stream, CancellationToken token) =>
        serializer.Deserialize(stream, token);
}
