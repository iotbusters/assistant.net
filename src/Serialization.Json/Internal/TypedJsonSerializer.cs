using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class TypedJsonSerializer<TValue> : ISerializer<TValue>
    {
        private readonly IJsonSerializer serializer;

        public TypedJsonSerializer(IJsonSerializer serializer) =>
            this.serializer = serializer;

        public Task Serialize(Stream stream, TValue value) => serializer.Serialize(stream, value!);

        public Task<TValue> Deserialize(Stream stream) => serializer.Deserialize(stream, typeof(TValue)).MapSuccess(x => (TValue) x);
    }
}