using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Internal;
using JsonSerializer = Assistant.Net.Serialization.Internal.JsonSerializer;

namespace Assistant.Net.Serialization
{
    public static class SerializerBuilderExtensions
    {
        public static SerializerBuilder AddJsonType<TValue>(this SerializerBuilder builder) => builder
            .AddJsonType(typeof(TValue));

        public static SerializerBuilder AddJsonType(this SerializerBuilder builder, Type serializingType)
        {
            if (serializingType.IsAssignableTo(typeof(Exception)))
                throw new ArgumentException("Invalid method for exception type.");

            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            var implementationType = typeof(TypedJsonSerializer<>).MakeGenericType(serializingType);

            builder.Services
                .TryAddSingleton<IJsonSerializer, JsonSerializer>()
                .ReplaceScoped(serviceType, implementationType);

            return builder;
        }

        public static SerializerBuilder AddJsonTypeAny(this SerializerBuilder builder)
        {
            builder.Services
                .TryAddSingleton<IJsonSerializer, JsonSerializer>()
                .ReplaceScoped(typeof(ISerializer<>), typeof(TypedJsonSerializer<>));
            return builder;
        }

        public static SerializerBuilder AddJsonConverter<TConverter>(this SerializerBuilder builder)
            where TConverter : JsonConverter
        {
            builder.Services
                .TryAddScoped<TConverter>()
                .Configure<JsonSerializerOptions, TConverter>((options, converter) => options
                    .Converters.Add(converter));
            return builder;
        }
    }
}