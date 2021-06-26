using System.IO;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Abstractions
{
    public static class SerializerExtensions
    {
        public static ISerializer<TValue> Create<TValue>(this ISerializerFactory factory) =>
            (ISerializer<TValue>) factory.Create(typeof(ISerializer<TValue>));

        public static async Task<byte[]> Serialize<TValue>(this ISerializer<TValue> serializer, TValue value)
        {
            var stream = new MemoryStream();
            await serializer.Serialize(stream, value);
            return stream.ToArray();
        }

        public static Task<TValue> Deserialize<TValue>(this ISerializer<TValue> serializer, byte[] bytes) => serializer
            .Deserialize(new MemoryStream(bytes));
    }
}