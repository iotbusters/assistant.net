using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class TypedJsonSerializer<TValue> : ISerializer<TValue>
    {
        private readonly IJsonSerializer serializer;

        public TypedJsonSerializer(IJsonSerializer serializer) =>
            this.serializer = serializer;

        public TValue Deserialize(byte[] bytes) =>
            (TValue) serializer.Deserialize(bytes, typeof(TValue));

        public byte[] Serialize(TValue value) =>
            serializer.Serialize(value!);
    }
}