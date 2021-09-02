using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal
{
    internal class DefaultJsonSerializer : IJsonSerializer
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public DefaultJsonSerializer(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        public Task<object> Deserialize(Stream stream, Type type, CancellationToken token) =>
            JsonSerializer.DeserializeAsync(stream, type, options.Value, token).AsTask()!;

        public Task Serialize(Stream stream, object value, CancellationToken token) =>
            JsonSerializer.SerializeAsync(stream, value, options.Value, token);
    }
}