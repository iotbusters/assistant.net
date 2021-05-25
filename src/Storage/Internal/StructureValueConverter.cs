using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Utils;

namespace Assistant.Net.Storage.Internal
{
    internal class StructureValueConverter<TValue> : IValueBinaryConverter<TValue> where TValue : struct
    {
        public byte[] Convert(TValue value) => value.Serialize();

        public TValue Convert(byte[] value) => value.Deserialize<TValue>();
    }
}