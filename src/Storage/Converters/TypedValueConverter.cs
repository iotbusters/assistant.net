using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Storage.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters;

internal class TypedValueConverter<TValue> : IValueConverter<TValue>
{
    private readonly ISerializer<TValue> serializer;

    public TypedValueConverter(ISerializer<TValue> serializer) => this.serializer = serializer;

    public Task<byte[]> Convert(TValue value, CancellationToken token) => serializer.Serialize(value, token);

    public Task<TValue> Convert(byte[] bytes, CancellationToken token) => serializer.Deserialize(bytes, token);
}
