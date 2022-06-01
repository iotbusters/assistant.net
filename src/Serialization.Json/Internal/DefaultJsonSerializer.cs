using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal;

internal class DefaultJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions options;

    public DefaultJsonSerializer(INamedOptions<JsonSerializerOptions> options) =>
        this.options = options.Value;

    public Task<object> Deserialize(Stream stream, Type type, CancellationToken token) =>
        JsonSerializer.DeserializeAsync(stream, type, options, token).AsTask()!;

    public Task Serialize(Stream stream, object value, CancellationToken token) =>
        JsonSerializer.SerializeAsync(stream, value, options, token);
}
