using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Converters;

/// <summary>
///     JSON converter factory for advanced serialization.
/// </summary>
public class AdvancedJsonConverterFactory : JsonConverterFactory
{
    private readonly IServiceProvider provider;

    /// <inheritdoc/>
    public AdvancedJsonConverterFactory(IServiceProvider provider) =>
        this.provider = provider;

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (EnumerableJsonConverter.CanConvert(typeToConvert, out var itemType))
            return CanConvert(itemType!);

        if (ExceptionJsonConverter.CanConvert(typeToConvert))
            return true;

        return AdvancedJsonConverter.CanConvert(typeToConvert);
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converters = options.Converters.SkipWhile(x => x != this).Skip(1).ToArray();
        var converter = converters.FirstOrDefault(converter => converter.CanConvert(typeToConvert));
        if (converter != null)
            return converter;

        if (EnumerableJsonConverter.CanConvert(typeToConvert, out var itemType))
        {
            var converterType = typeof(EnumerableJsonConverter<>).MakeGenericType(itemType!);
            return (JsonConverter?)provider.GetRequiredService(converterType);
        }

        if (ExceptionJsonConverter.CanConvert(typeToConvert))
        {
            var converterType = typeof(ExceptionJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)provider.GetRequiredService(converterType);
        }

        if (AdvancedJsonConverter.CanConvert(typeToConvert))
        {
            var converterType = typeof(AdvancedJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)provider.GetRequiredService(converterType);
        }

        return null;
    }
}
