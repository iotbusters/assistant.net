using Assistant.Net.Serialization.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Converters;

internal static class EnumerableJsonConverter
{
    public static bool CanConvert(Type typeToConvert, out Type? itemType)
    {
        itemType = GetSequenceItemType(typeToConvert);
        return itemType != null && !IsSystemType(itemType);
    }

    private static Type? GetSequenceItemType(Type sequenceType) =>
        sequenceType.GetElementType() ?? sequenceType.GetInterfaces().Append(sequenceType)
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(x => x.GetGenericArguments().Single())
            .FirstOrDefault();
    public  static bool IsSystemType(Type type) => type.Namespace != null && type.Namespace.StartsWith("System");
}

/// <summary>
///     Json converter responsible for enumerable objects serialization.
/// </summary>
/// <typeparam name="T">Enumerated, non-system item type.</typeparam>
public sealed class EnumerableJsonConverter<T> : JsonConverter<IEnumerable<T>>
{
    /// <summary/>
    public EnumerableJsonConverter()
    {
        if (EnumerableJsonConverter.IsSystemType(typeof(T)))
            throw new JsonException($"The type '{typeof(T)}' isn't supported.");
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => EnumerableJsonConverter.CanConvert(typeToConvert, out _);

    /// <inheritdoc/>
    /// <exception cref="NotResolvedJsonException"/>
    /// <exception cref="JsonException"/>
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
        catch (Exception ex)
        {
            throw new NotResolvedJsonException(
                typeToConvert.Name,
                $"The type '{typeToConvert}' failed to deserialize.",
                ex);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="JsonException"/>
    public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
    {
        // todo: remove?
        if (!IsSupportedSequenceType(value.GetType()))
            throw new JsonException($"The type '{typeof(T)}' isn't supported.");

        var arrayItemType = GetSequenceItemType(value.GetType())!;
        writer.WriteStartArray();

        foreach (var item in value)
            JsonSerializer.Serialize(writer, item, arrayItemType, options);

        writer.WriteEndArray();
    }

    // todo: move into EnumerableJsonConverter.CanConvert?
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

    // todo: move into EnumerableJsonConverter.CanConvert?
    private static Type? GetSequenceItemType(Type sequenceType) =>
        sequenceType.GetElementType() ?? sequenceType.GetInterfaces().Append(sequenceType)
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(x => x.GetGenericArguments().Single())
            .FirstOrDefault();
}
