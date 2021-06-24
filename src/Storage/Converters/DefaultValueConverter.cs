using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Converters
{
    internal class DefaultValueConverter<TValue> : IValueConverter<TValue>
    {
        private readonly ISerializer<TValue> serializer;

        public DefaultValueConverter(ISerializer<TValue> serializer) =>
            this.serializer = serializer;

        public byte[] Convert(TValue value) => serializer.Serialize(value);

        public TValue Convert(byte[] bytes) => serializer.Deserialize(bytes);
    }
}