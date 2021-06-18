using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Converters
{
    public class EnumerableJsonConverter : JsonConverter<IEnumerable>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert != typeof(string)
            && GetSequenceItemType(typeToConvert) != null;

        public override IEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(typeToConvert == typeof(IEnumerable))
                throw new JsonException("System.Test.Json converters provided invalid type for a sequence type.");

            if (typeToConvert.IsAssignableTo(typeof(IEnumerable<byte>)))
                return reader.GetBytesFromBase64();
            if (typeToConvert.IsAssignableTo(typeof(IEnumerable<char>)))
                return reader.GetString()!;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"'{JsonTokenType.StartArray}' token is expected but found '{reader.TokenType}'.");

            var arrayItemType = GetSequenceItemType(typeToConvert)!;
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
                throw new InstantiateFailedJsonException(typeToConvert.Name, $"Failed to instantiate type '{typeToConvert}'.", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable value, JsonSerializerOptions options)
        {
            if (value is IEnumerable<byte> bytes)
            {
                writer.WriteBase64StringValue(bytes.ToArray());
                return;
            }

            if (value is IEnumerable<char> text)
            {
                writer.WriteStringValue(new string(text.ToArray()));
                return;
            }

            var arrayItemType = GetSequenceItemType(value.GetType())!;
            writer.WriteStartArray();

            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, arrayItemType, options);

            writer.WriteEndArray();
        }

        private static Type? GetSequenceItemType(Type sequenceType) =>
            sequenceType.GetElementType() ?? sequenceType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(x => x.GetGenericArguments().Single())
                .FirstOrDefault();
    }
}