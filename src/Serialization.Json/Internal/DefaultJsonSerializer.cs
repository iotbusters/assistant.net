using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class DefaultJsonSerializer : IJsonSerializer
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public DefaultJsonSerializer(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        public Task<object> Deserialize(Stream stream, Type type) =>
            JsonSerializer.DeserializeAsync(stream, type, options.Value).AsTask()!;

        public Task Serialize(Stream stream, object value) =>
            JsonSerializer.SerializeAsync(stream, value, options.Value);
    }
}