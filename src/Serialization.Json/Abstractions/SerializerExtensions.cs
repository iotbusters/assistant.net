using System.IO;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     Serializer extensions.
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        ///     Serializes the <paramref name="value"/> to a byte array.
        /// </summary>
        public static async Task<byte[]> Serialize<TValue>(this ISerializer<TValue> serializer, TValue value)
        {
            var stream = new MemoryStream();
            await serializer.Serialize(stream, value);
            return stream.ToArray();
        }

        /// <summary>
        ///     Deserializes a byte array to <typeparamref name="TValue object"/>.
        /// </summary>
        public static Task<TValue> Deserialize<TValue>(this ISerializer<TValue> serializer, byte[] bytes) => serializer
            .Deserialize(new MemoryStream(bytes));

        /// <summary>
        ///     Deserializes a byte array to <typeparamref name="TValue object"/>.
        /// </summary>
        public static async Task<TValue> Deserialize<TValue>(this ISerializer<object> serializer, byte[] bytes) =>
            (TValue) await serializer.Deserialize(new MemoryStream(bytes));
    }
}