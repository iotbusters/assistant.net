using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal
{
    internal class TypedJsonSerializer<TValue> : ISerializer<TValue>
    {
        private readonly IJsonSerializer serializer;

        public TypedJsonSerializer(IJsonSerializer serializer) =>
            this.serializer = serializer;

        public Task Serialize(Stream stream, TValue value) => serializer.Serialize(stream, value!);

        public async Task<TValue> Deserialize(Stream stream) =>
            (TValue) await serializer.Deserialize(stream, typeof(TValue));
    }
}