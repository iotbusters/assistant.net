using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions;

/// <summary>
///     A de-typed abstraction over binary serializer of object.
/// </summary>
public interface IAbstractSerializer
{
    /// <summary>
    ///     Serializes <paramref name="value"/> object to <paramref name="stream"/>.
    /// </summary>
    Task SerializeObject(Stream stream, object value, CancellationToken token = default);

    /// <summary>
    ///     Deserializes <paramref name="stream"/> to object.
    /// </summary>
    Task<object> DeserializeObject(Stream stream, CancellationToken token = default);
}