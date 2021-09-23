using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Converters
{
    internal static class AdvancedJsonConverter
    {
        public static bool CanConvert(Type typeToConvert) => !IsSystemType(typeToConvert);

        private static bool IsSystemType(Type type) => type.Namespace != null && type.Namespace.StartsWith("System");
    }

    /// <summary>
    ///     Json converter responsible for advanced object serialization.
    /// </summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to
    public sealed class AdvancedJsonConverter<T> : JsonConverter<T>
    {
        private const string TypePropertyName = "__type";

        private readonly ConcurrentDictionary<Type, IImmutableList<TypeMetadata>> typeMetadata = new();
        private readonly ITypeEncoder typeEncoder;

        /// <summary/>
        /// <exception cref="JsonException"/>
        public AdvancedJsonConverter(ITypeEncoder typeEncoder) =>
            this.typeEncoder = typeEncoder;

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsAssignableTo(typeof(T)) && AdvancedJsonConverter.CanConvert(typeToConvert);

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            var type = value.GetType();
            var typeName = GetTypeName(type);
            var metadata = GetTypeMetadata(type);

            writer.WriteString(TypePropertyName, typeName);

            var getters = metadata.OrderByDescending(x => x.Getters.Count).First().Getters.Values;
            foreach (var getter in getters)
            {
                var propertyValue = getter.GetValue(value);
                if (propertyValue == null && !options.IgnoreNullValues)
                    continue;

                writer.WritePropertyName(getter.Name);

                var propertyValueType = propertyValue?.GetType() ?? getter.PropertyType;
                JsonSerializer.Serialize(writer, propertyValue, propertyValueType, options);
            }

            writer.WriteEndObject();
        }

        /// <inheritdoc/>
        /// <exception cref="TypeResolvingFailedJsonException"/>
        /// <exception cref="JsonException"/>
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default!;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Start object token is expected.");

            var typeName = ReadTypeName(ref reader);
            var type = GetTypeFromName(typeName);
            if (!CanConvert(type))
                throw new JsonException($"Type '{typeToConvert}' or one of its derived types are expected but found '{type}'.");

            var metadata = GetTypeMetadata(type);
            var foundValues = ReadValues(ref reader, metadata, options);

            var initializationSets = (from m in metadata
                                      let cvs = (from n in m.CtorArguments
                                                 select foundValues.TryGetValue(n, out var v) ? v : default).ToArray()
                                      let pvs = (from n in m.Setters.Keys
                                                 where foundValues.ContainsKey(n)
                                                 select new
                                                 {
                                                     property = m.Setters[n],
                                                     value = foundValues[n]
                                                 }).ToImmutableList()
                                      let isAllCtorArgumentsFound = m.CtorArguments.Except(foundValues.Keys).Any()
                                      let requiredPropertiesCount = cvs.Length + pvs.Count
                                      where requiredPropertiesCount >= foundValues.Count
                                      orderby
                                          isAllCtorArgumentsFound descending, // full or partial ctor match.
                                          requiredPropertiesCount descending // the more parameters the better.
                                      select new
                                      {
                                          m.Ctor,
                                          ctorValues = cvs,
                                          setterValues = pvs
                                      }).ToImmutableList();

            var initializationErrors = new List<Exception>();
            foreach (var initializationSet in initializationSets)
            {
                try
                {
                    var valueToConvert = initializationSet.Ctor.Invoke(initializationSet.ctorValues);

                    foreach (var setterValue in initializationSet.setterValues)
                        setterValue.property.SetValue(valueToConvert, setterValue.value);

                    return (T)valueToConvert;
                }
                catch (Exception e)
                {
                    initializationErrors.Add(e);
                }
            }

            throw new TypeResolvingFailedJsonException(
                typeName,
                $"The type '{typeof(T)}' failed to deserialize.",
                new AggregateException(initializationErrors));
        }

        private IImmutableList<TypeMetadata> GetTypeMetadata(Type type) => typeMetadata.GetOrAdd(
            type,
            key =>
            {
                var typeProperties = key.GetProperties().ToImmutableDictionary(x => x.Name, IgnoreCase());
                var metadata = (from ctor in key.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                let cParams = (from p in ctor.GetParameters() select p.Name!).ToImmutableList()
                                let cParamsFound = (from p in ctor.GetParameters()
                                                    let n = p.Name!
                                                    let t = p.ParameterType
                                                    where typeProperties.ContainsKey(n)
                                                    where typeProperties[n].CanRead
                                                    where typeProperties[n].PropertyType.IsAssignableFrom(t)
                                                          || typeProperties[n].PropertyType.IsAssignableTo(t)
                                                    select n).ToImmutableList()
                                let setters = (from n in typeProperties.Keys.Except(cParamsFound, IgnoreCase())
                                               where typeProperties[n].CanRead && typeProperties[n].CanWrite
                                               select n).ToImmutableDictionary(x => x, x => typeProperties[x], IgnoreCase())
                                let getters = setters.Keys.Concat(cParamsFound)
                                    .ToImmutableDictionary(x => x, x => typeProperties[x], IgnoreCase())
                                where cParams.Count == cParamsFound.Count // all ctor parameters are resolvable.
                                select new TypeMetadata(ctor, cParams, getters, setters)).ToImmutableList();

                if (!metadata.Any())
                    throw new JsonException($"The type '{type}' cannot be serialized or deserialized"
                                            + " because it cannot deserialize all serializable members.");
                return metadata;
            });

        private string GetTypeName(Type type) => typeEncoder.Encode(type)
                                                 ?? throw new JsonException($"Type '{type}' isn't supported.");

        /// <exception cref="JsonException"/>
        private Type GetTypeFromName(string typeName) => typeEncoder.Decode(typeName)
                                                         ?? throw new JsonException($"Type '{typeName}' wasn't found.");

        private static StringComparer IgnoreCase() => StringComparer.InvariantCultureIgnoreCase;

        /// <exception cref="JsonException"/>
        private static string ReadTypeName(ref Utf8JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"'{JsonTokenType.PropertyName}' token is expected but found '{reader.TokenType}'.");

            var propertyName = reader.GetString()!;
            if (propertyName != TypePropertyName)
                throw new JsonException($"Property '{TypePropertyName}' is expected but found '{propertyName}'.");

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("String token is expected.");

            return reader.GetString()!;
        }

        /// <exception cref="JsonException"/>
        private static IImmutableDictionary<string, object?> ReadValues(
            ref Utf8JsonReader reader,
            IImmutableList<TypeMetadata> metadata,
            JsonSerializerOptions options)
        {
            var knownProperties = metadata
                .SelectMany(x => x.Getters)
                .Distinct()
                .ToImmutableDictionary(x => x.Key, x => x.Value.PropertyType, IgnoreCase());
            var foundValues = new Dictionary<string, object?>();

            for (reader.Read(); reader.TokenType != JsonTokenType.EndObject; reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"'{JsonTokenType.PropertyName}' token is expected but found '{reader.TokenType}'.");

                var propertyName = reader.GetString()!;
                if (!knownProperties.TryGetValue(propertyName, out var type))
                    throw new JsonException($"Unknown property '{propertyName}' isn't supported.");

                reader.Read();
                try
                {
                    var value = JsonSerializer.Deserialize(ref reader, type, options);
                    foundValues.Add(propertyName, value);
                }
                catch (Exception e)
                {
                    throw new JsonException($"Failed to read property '{propertyName}' value.", e);
                }
            }

            return foundValues.ToImmutableDictionary(IgnoreCase());
        }
    }
}
