using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Internal;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization
{
    /// <summary>
    ///     Serializer builder extensions for configuring JSON serialization.
    /// </summary>
    public static class SerializerBuilderExtensions
    {
        /// <summary>
        ///     Configures JSON serialization of <typeparamref name="TValue"/> type.
        /// </summary>
        /// <typeparam name="TValue">Serializing value type.</typeparam>
        public static SerializerBuilder AddJsonType<TValue>(this SerializerBuilder builder) => builder
            .AddJsonType(typeof(TValue));

        /// <summary>
        ///     Configures JSON serialization of <paramref name="serializingType"/>.
        /// </summary>
        public static SerializerBuilder AddJsonType(this SerializerBuilder builder, Type serializingType)
        {
            if (serializingType.IsAssignableTo(typeof(Exception)))
                throw new ArgumentException("Invalid method for exception type.");

            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            var implementationType = typeof(TypedJsonSerializer<>).MakeGenericType(serializingType);

            builder.Services
                .TryAddSingleton<IJsonSerializer, DefaultJsonSerializer>()
                .ReplaceSingleton(serviceType, implementationType);

            return builder;
        }

        /// <summary>
        ///     Configures JSON serialization of all types which weren't configured explicitly.
        /// </summary>
        public static SerializerBuilder AddJsonTypeAny(this SerializerBuilder builder)
        {
            builder.Services
                .TryAddSingleton<IJsonSerializer, DefaultJsonSerializer>()
                .ReplaceSingleton(typeof(ISerializer<>), typeof(TypedJsonSerializer<>));
            return builder;
        }

        /// <summary>
        ///     Adds type converter for JSON serialization.
        /// </summary>
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