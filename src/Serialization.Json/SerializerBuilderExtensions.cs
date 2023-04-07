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
    ///     Configures single JSON serializer.
    /// </summary>
    public static SerializerBuilder UseJson(this SerializerBuilder builder) => builder
        .UseJson(delegate { });

    /// <summary>
    ///     Configures single JSON serializer.
    /// </summary>
    public static SerializerBuilder UseJson(this SerializerBuilder builder, Action<JsonSerializerOptions> configureOptions)
    {
        builder.Services.ConfigureJsonSerializer(builder.Name, configureOptions);
        return builder
            .UseFormat((provider, serializingType) =>
            {
                var implementationType = typeof(DefaultJsonSerializer<>).MakeGenericType(serializingType);
                return (IAbstractSerializer)provider.Create(implementationType);
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
    ///     Registers named default JSON configuration and dependencies.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action to configure options.</param>
    private static void ConfigureJsonSerializer(this IServiceCollection services, string name, Action<JsonSerializerOptions> configureOptions) => services
        .AddTypeEncoder()
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
            options.Converters.TryAdd(converter))
        .Configure(name, configureOptions);
}
