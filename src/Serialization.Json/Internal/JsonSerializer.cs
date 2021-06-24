using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class JsonSerializer : IJsonSerializer
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public JsonSerializer(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        public object Deserialize(byte[] bytes, Type type) =>
            System.Text.Json.JsonSerializer.Deserialize(bytes, type, options.Value)!;

        public byte[] Serialize(object value)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value, options.Value);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}