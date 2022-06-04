using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Internal;
using Assistant.Net.Serialization.Options;
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
    ///     Adds <see cref="ISerializer{TValue}"/> implementation, required services and defaults.
    /// </summary>
    /// <remarks>
    ///     Pay attention, all serializing types should be separately registered.
    /// </remarks>
    public static IServiceCollection AddSerializer(this IServiceCollection services) => services
        .AddTypeEncoder()
        .AddNamedOptionsContext()
        .TryAddScoped<ISerializerFactory, SerializerFactory>()
        .TryAddScoped(typeof(ISerializer<>), typeof(DefaultSerializer<>));

    /// <summary>
    ///     Adds default <see cref="ISerializer{TValue}"/> implementation configured for specific values.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the builder.</param>
    public static IServiceCollection AddSerializer(this IServiceCollection services, Action<SerializerBuilder> configure) => services
        .AddSerializer()
        .ConfigureSerializer(configure)
        .ConfigureSerializerOptions(delegate { });

    /// <summary>
    ///     Adds the same named <see cref="ISerializer{TValue}"/> implementation configured for specific values.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the builder instance.</param>
    /// <param name="configure">The action used to configure the builder.</param>
    public static IServiceCollection AddSerializer(this IServiceCollection services, string name, Action<SerializerBuilder> configure) => services
        .AddSerializer()
        .ConfigureSerializer(name, configure)
        .ConfigureSerializerOptions(name, delegate { });

    /// <summary>
    ///     Configures default <see cref="ISerializer{TValue}" /> implementation for specific values.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureSerializer(this IServiceCollection services, Action<SerializerBuilder> configure) => services
        .ConfigureSerializer(Microsoft.Extensions.Options.Options.DefaultName, configure);

    /// <summary>
    ///     Configures the same named <see cref="ISerializer{TValue}" /> implementation for specific values.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the builder instance.</param>
    /// <param name="configure">The action used to configure the builder.</param>
    public static IServiceCollection ConfigureSerializer(this IServiceCollection services, string name, Action<SerializerBuilder> configure)
    {
        configure(new SerializerBuilder(services, name));
        return services;
    }

    /// <summary>
    ///     Register an action used to configure default <see cref="SerializerOptions"/> options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSerializerOptions(this IServiceCollection services, Action<SerializerOptions> configureOptions) => services
        .ConfigureSerializerOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="SerializerOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureSerializerOptions(this IServiceCollection services, string name, Action<SerializerOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Register an action used to configure default <see cref="JsonSerializerOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions) => services
        .ConfigureJsonSerializerOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="JsonSerializerOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureJsonSerializerOptions(this IServiceCollection services, string name, Action<JsonSerializerOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Adds new converter unless the same converter is already added.
    /// </summary>
    /// <param name="converters">Target JSON converter list.</param>
    /// <param name="converter">The JSON converter.</param>
    public static IList<JsonConverter> TryAdd(this IList<JsonConverter> converters, JsonConverter converter)
    {
        var converterType = converter.GetType();
        if (converters.Any(x => x.GetType() == converterType))
            return converters;

        converters.Add(converter);
        return converters;
    }
}
