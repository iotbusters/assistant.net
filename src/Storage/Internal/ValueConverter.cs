using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Utils;

namespace Assistant.Net.Storage.Internal
{
    internal class ValueConverter<TValue> : IValueBinaryConverter<TValue>
    {
        public byte[] Convert(TValue value) => value.Serialize();

        public TValue Convert(byte[] bytes) => bytes.Deserialize<TValue>();
    }
}