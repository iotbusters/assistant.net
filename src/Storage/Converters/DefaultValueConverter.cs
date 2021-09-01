using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Storage.Abstractions;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters
{
    internal class DefaultValueConverter<TValue> : IValueConverter<TValue>
    {
        private readonly ISerializer<TValue> serializer;

        public DefaultValueConverter(ISerializer<TValue> serializer) =>
            this.serializer = serializer;

        public Task<byte[]> Convert(TValue value) => serializer.Serialize(value);

        public Task<TValue> Convert(byte[] bytes) => serializer.Deserialize(bytes);
    }
}