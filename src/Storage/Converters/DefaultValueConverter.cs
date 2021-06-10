using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;

namespace Assistant.Net.Storage.Converters
{
    internal class DefaultValueConverter<TValue> : IValueConverter<TValue>
    {
        public byte[] Convert(TValue value) => value.Serialize();

        public TValue Convert(byte[] bytes) => bytes.Deserialize<TValue>();
    }
}