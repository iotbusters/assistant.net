using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Storage.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters
{
    internal class DefaultKeyConverter<TKey> : IKeyConverter<TKey>
    {
        private readonly ISerializer<TKey> serializer;

        public DefaultKeyConverter(ISerializer<TKey> serializer) => this.serializer = serializer;

        public Task<byte[]> Convert(TKey key, CancellationToken token) => serializer.Serialize(key!, token);

        public Task<TKey> Convert(byte[] keyContent, CancellationToken token) => serializer.Deserialize(keyContent, token);
    }
}