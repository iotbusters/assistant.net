using System;
using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class DefaultSerializer<TValue> : ISerializer<TValue>
    {
        private readonly Lazy<ISerializer<TValue>> serializer;

        public DefaultSerializer(ISerializerFactory factory) =>
            serializer = new Lazy<ISerializer<TValue>>(factory.Create<TValue>);

        public Task Serialize(Stream stream, TValue value) => serializer.Value.Serialize(stream, value!);

        public Task<TValue> Deserialize(Stream stream) => serializer.Value.Deserialize(stream);
    }
}