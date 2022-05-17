using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization;

/// <summary>
/// Service collection extensions for serialization.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers default serializer services only.
    /// </summary>
    /// <remarks>
    ///     Pay attention, all serializing types should be separately registered.
    /// </remarks>
    public static IServiceCollection AddSerializer(this IServiceCollection services) => services
        .AddSerializer(delegate { });

    /// <summary>
    ///     Registers default configuration for <see cref="ISerializer{TValue}" /> customized by <paramref name="configure" /> action.
    /// </summary>
    public static IServiceCollection AddSerializer(this IServiceCollection services, Action<SerializerBuilder> configure) => services
        .AddTypeEncoder()
        .TryAddSingleton<ISerializerFactory, SerializerFactory>()
        .TryAddSingleton(typeof(ISerializer<>), typeof(UnknownSerializer<>))
        .TryAddSingleton<AdvancedJsonConverterFactory>()
        .TryAddSingleton(typeof(ExceptionJsonConverter<>), typeof(ExceptionJsonConverter<>))
        .TryAddSingleton(typeof(EnumerableJsonConverter<>), typeof(EnumerableJsonConverter<>))
        .TryAddSingleton(typeof(AdvancedJsonConverter<>), typeof(AdvancedJsonConverter<>))
        .Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.TryAdd(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        })
        .ConfigureSerializer(configure)
        .Configure<JsonSerializerOptions, ExceptionJsonConverter<Exception>>((options, converter) => options
            .Converters.TryAdd(converter))
        .Configure<JsonSerializerOptions, AdvancedJsonConverterFactory>((options, converter) => options
            .Converters.TryAdd(converter));

    /// <summary>
    ///     Configures <see cref="ISerializer{TValue}" /> implementations for specific values.
    /// </summary>
    public static IServiceCollection ConfigureSerializer(this IServiceCollection services, Action<SerializerBuilder> configure)
    {
        configure(new SerializerBuilder(services));
        return services;
    }

    /// <summary>
    ///     Register an action used to configure the <see cref="JsonSerializerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureJsonSerializationOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///     Adds new converter unless the same converter is already added.
    /// </summary>
    public static IList<JsonConverter> TryAdd(this IList<JsonConverter> converters, JsonConverter converter)
    {
        var converterType = converter.GetType();
        if (converters.Any(x => x.GetType() == converterType))
            return converters;

        converters.Add(converter);
        return converters;
    }
}