using System;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class DelegatingAbstractSerializer : ISerializer<object>
    {
        private readonly Func<object, byte[]> serialize;
        private readonly Func<byte[], object> deserialize;

        public DelegatingAbstractSerializer(Func<object,byte[]> serialize, Func<byte[], object> deserialize)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public byte[] Serialize(object value) => serialize(value);

        public object Deserialize(byte[] bytes) => deserialize(bytes);
    }
}