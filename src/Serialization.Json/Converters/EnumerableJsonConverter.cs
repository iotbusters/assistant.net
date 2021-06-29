using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Converters
{
    public sealed class EnumerableJsonConverter<T> : JsonConverter<IEnumerable<T>>
    {
        public EnumerableJsonConverter()
        {
            if (AdvancedJsonConverterFactory.IsSystemType(typeof(T)))
                throw new JsonException($"The type '{typeof(T)}' isn't supported.");
        }

        public override bool CanConvert(Type typeToConvert)
        {
            var itemType = AdvancedJsonConverterFactory.GetSequenceItemType(typeToConvert);
            return itemType != null && !AdvancedJsonConverterFactory.IsSystemType(itemType);
        }

        public override IEnumerable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"'{JsonTokenType.StartArray}' token is expected but found '{reader.TokenType}'.");

            var arrayItemType = AdvancedJsonConverterFactory.GetSequenceItemType(typeToConvert)!;
            dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(arrayItemType))!;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                list.Add((dynamic)JsonSerializer.Deserialize(ref reader, arrayItemType, options)!);
            reader.Read();

            if (typeToConvert.IsInstanceOfType(list))
                return list;

            if (typeToConvert.IsArray)
                return list.ToArray();

            try
            {
                return Activator.CreateInstance(typeToConvert, list.ToArray())!;
            }
            catch (Exception e)
            {
                throw new TypeResolvingFailedJsonException(typeToConvert.Name, $"Failed to instantiate type '{typeToConvert}'.", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
        {
            var arrayItemType = AdvancedJsonConverterFactory.GetSequenceItemType(value.GetType())!;
            writer.WriteStartArray();

            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, arrayItemType, options);

            writer.WriteEndArray();
        }
    }
}