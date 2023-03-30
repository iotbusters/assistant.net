using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization;

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
    /// <exception cref="ArgumentException"/>
    public static SerializerBuilder AddJsonType(this SerializerBuilder builder, Type serializingType)
    {
        builder.Services.AddJsonSerializer(builder.Name);
        var implementationType = typeof(TypedJsonSerializer<>).MakeGenericType(serializingType);
        return builder.AddType(serializingType, implementationType);
    }

    /// <summary>
    ///     Configures JSON serialization of all types which weren't configured explicitly.
    /// </summary>
    public static SerializerBuilder AddJsonTypeAny(this SerializerBuilder builder)
    {
        builder.Services.AddJsonSerializer(builder.Name);
        return builder.AddTypeAny((p, serializingType) =>
        {
            var implementationType = typeof(TypedJsonSerializer<>).MakeGenericType(serializingType);
            return (IAbstractSerializer)p.GetRequiredService(implementationType);
        });
    }

    /// <summary>
    ///     Adds type converter for JSON serialization.
    /// </summary>
    public static SerializerBuilder AddJsonConverter<TConverter>(this SerializerBuilder builder)
        where TConverter : JsonConverter
    {
        builder.Services
            .TryAddScoped<TConverter>()
            .Configure<JsonSerializerOptions, IServiceProvider>(builder.Name, (o, p) =>
                o.Converters.Add(p.GetRequiredService<TConverter>()));
        return builder;
    }

    /// <summary>
    ///     Registers named <see cref="IJsonSerializer"/>, default configuration and its dependencies.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddJsonSerializer(this IServiceCollection services, string name) => services
        .AddTypeEncoder()
        .TryAddScoped(typeof(TypedJsonSerializer<>), typeof(TypedJsonSerializer<>))
        .TryAddScoped<IJsonSerializer, DefaultJsonSerializer>()
        .TryAddScoped<AdvancedJsonConverterFactory>()
        .TryAddSingleton(typeof(ExceptionJsonConverter<>), typeof(ExceptionJsonConverter<>))
        .TryAddSingleton(typeof(EnumerableJsonConverter<>), typeof(EnumerableJsonConverter<>))
        .TryAddSingleton(typeof(AdvancedJsonConverter<>), typeof(AdvancedJsonConverter<>))
        .Configure<JsonSerializerOptions>(name, options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.TryAdd(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        })
        .Configure<JsonSerializerOptions, AdvancedJsonConverterFactory>(name, (options, converter) =>
            options.Converters.TryAdd(converter));
}
