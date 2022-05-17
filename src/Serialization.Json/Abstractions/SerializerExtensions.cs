using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions;

/// <summary>
///     Serializer extensions.
/// </summary>
public static class SerializerExtensions
{
    /// <summary>
    ///     Serializes the <paramref name="value"/> to a byte array.
    /// </summary>
    public static async Task<byte[]> Serialize(this IAbstractSerializer serializer, object value, CancellationToken token = default)
    {
        var stream = new MemoryStream();
        await serializer.SerializeObject(stream, value, token);
        return stream.ToArray();
    }

    /// <summary>
    ///     Deserializes a byte array to object.
    /// </summary>
    public static Task<object> Deserialize(this IAbstractSerializer serializer, byte[] bytes, CancellationToken token = default) =>
        serializer.DeserializeObject(new MemoryStream(bytes), token);

    /// <summary>
    ///     Serializes the <paramref name="value"/> to a byte array.
    /// </summary>
    public static async Task<byte[]> Serialize<TValue>(this ISerializer<TValue> serializer, TValue value, CancellationToken token = default)
    {
        var stream = new MemoryStream();
        await serializer.Serialize(stream, value, token);
        return stream.ToArray();
    }

    /// <summary>
    ///     Deserializes a byte array to <typeparamref name="TValue"/>.
    /// </summary>
    public static Task<TValue> Deserialize<TValue>(this ISerializer<TValue> serializer, byte[] bytes, CancellationToken token = default) =>
        serializer.Deserialize(new MemoryStream(bytes), token);

    /// <summary>
    ///     Deserializes a byte array to <typeparamref name="TValue"/>.
    /// </summary>
    public static async Task<TValue> Deserialize<TValue>(this ISerializer<object> serializer, byte[] bytes, CancellationToken token = default) =>
        (TValue) await serializer.Deserialize(new MemoryStream(bytes), token);
}