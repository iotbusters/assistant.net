using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal;

internal class DefaultJsonSerializer<TValue> : ISerializer<TValue>
{
    private readonly JsonSerializerOptions options;

    public DefaultJsonSerializer(INamedOptions<JsonSerializerOptions> options) =>
        this.options = options.Value;

    public Task Serialize(Stream stream, TValue value, CancellationToken token) =>
        JsonSerializer.SerializeAsync(stream, value, options, token);

    public Task<TValue> Deserialize(Stream stream, CancellationToken token) =>
        JsonSerializer.DeserializeAsync<TValue>(stream, options, token).AsTask()!;
}
