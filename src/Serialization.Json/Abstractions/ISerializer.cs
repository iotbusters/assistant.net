using System.IO;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     An abstraction over binary serializer of <typeparamref name="TValue" />.
    /// </summary>
    /// <typeparam name="TValue">Specific type to serialize.</typeparam>
    public interface ISerializer<TValue> : IAbstractSerializer
    {
        /// <summary>
        ///     Serializes <paramref name="value"/> object to <paramref name="stream"/>.
        /// </summary>
        Task Serialize(Stream stream, TValue value);

        /// <summary>
        ///     Deserializes <paramref name="stream"/> to <typeparamref name="TValue" /> object.
        /// </summary>
        Task<TValue> Deserialize(Stream stream);

        Task IAbstractSerializer.SerializeObject(Stream stream, object value) => Serialize(stream, (TValue) value);

        async Task<object> IAbstractSerializer.DeserializeObject(Stream stream) => (await Deserialize(stream))!;
    }
}