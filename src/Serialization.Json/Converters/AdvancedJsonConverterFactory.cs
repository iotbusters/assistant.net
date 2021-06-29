using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Serialization.Converters
{
    public class AdvancedJsonConverterFactory : JsonConverterFactory
    {
        private readonly IServiceProvider provider;

        public AdvancedJsonConverterFactory(IServiceProvider provider) =>
            this.provider = provider;

        public override bool CanConvert(Type typeToConvert)
        {
            var itemType = GetSequenceItemType(typeToConvert);
            if (itemType != null)
                return !IsSystemType(itemType);

            return !IsSystemType(typeToConvert);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var itemType = GetSequenceItemType(typeToConvert);
            if (itemType != null)
            {
                var converterType = typeof(EnumerableJsonConverter<>).MakeGenericType(itemType);
                return (JsonConverter?)provider.GetRequiredService(converterType);
            }

            if (!IsSystemType(typeToConvert))
            {
                var converterType = typeof(AdvancedJsonConverter<>).MakeGenericType(typeToConvert);
                return (JsonConverter?)provider.GetRequiredService(converterType);
            }

            return null;
        }

        internal static Type? GetSequenceItemType(Type sequenceType) =>
            sequenceType.GetElementType() ?? sequenceType.GetInterfaces().Append(sequenceType)
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(x => x.GetGenericArguments().Single())
                .FirstOrDefault();

        internal static bool IsSystemType(Type type) =>
            type.Namespace != null && type.Namespace.StartsWith("System");
    }
}