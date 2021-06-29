using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"'{JsonTokenType.StartArray}' token is expected but found '{reader.TokenType}'.");

            var list = new List<T>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                list.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
            reader.Read();

            if (typeToConvert.IsArray)
                return list.ToArray();
            if (typeToConvert == typeof(ImmutableArray<T>))
                return list.ToImmutableArray();
            if (typeToConvert.IsInstanceOfType(list))
                return list;
            if (typeToConvert.IsAssignableTo(typeof(IImmutableList<T>)))
                return list.ToImmutableList();
            if (typeToConvert.IsAssignableTo(typeof(IImmutableSet<T>)))
                return list.ToImmutableHashSet();
            if (typeToConvert.IsAssignableTo(typeof(IImmutableQueue<T>)))
                return ImmutableQueue.Create(list.ToArray());
            if (typeToConvert.IsAssignableTo(typeof(IImmutableStack<T>)))
                return ImmutableStack.Create(list.ToArray());

            try
            {
                return (IEnumerable<T>)Activator.CreateInstance(typeToConvert, list.ToArray())!;
            }
            catch (Exception e)
            {
                throw new TypeResolvingFailedJsonException(typeToConvert.Name, $"Failed to instantiate type '{typeToConvert}'.", e);
            }
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
        {
            if(!IsSupportedSequenceType(value.GetType()))
                throw new JsonException($"The type '{typeof(T)}' isn't supported.");

            var arrayItemType = AdvancedJsonConverterFactory.GetSequenceItemType(value.GetType())!;
            writer.WriteStartArray();

            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, arrayItemType, options);

            writer.WriteEndArray();
        }

        private static bool IsSupportedSequenceType(Type sequenceType) =>
            sequenceType.IsArray
            || sequenceType == typeof(ImmutableArray<T>)
            || sequenceType.IsInstanceOfType(typeof(List<T>))
            || sequenceType.IsAssignableTo(typeof(IImmutableList<T>))
            || sequenceType.IsAssignableTo(typeof(IImmutableSet<T>))
            || sequenceType.IsAssignableTo(typeof(IImmutableQueue<T>))
            || sequenceType.IsAssignableTo(typeof(IImmutableStack<T>))
            || sequenceType.GetConstructors().Any(x =>
                x.GetParameters().Length == 1
                && x.GetParameters().Single().ParameterType.IsAssignableFrom(typeof(T[])));
    }
}