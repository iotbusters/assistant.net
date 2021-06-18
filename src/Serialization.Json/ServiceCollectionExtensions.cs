using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Converters;

namespace Assistant.Net.Serialization
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers JSON serializer by default.
        /// </summary>
        public static IServiceCollection AddJsonSerialization(this IServiceCollection services) => services
            .AddSerializer(b => b.AddJsonAny());

        /// <summary>
        ///     Registers default configuration for <see cref="ISerializer{TValue}" /> customized by <paramref name="configure" /> action.
        /// </summary>
        public static IServiceCollection AddSerializer(this IServiceCollection services, Action<SerializerBuilder> configure) => services
            .AddTypeEncoder()
            .TryAddScoped<AdvancedJsonConverter>()
            .TryAddScoped<ExceptionJsonConverter>()
            .Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNameCaseInsensitive = true;
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            })
            .Configure<JsonSerializerOptions, AdvancedJsonConverter>((options, converter) => options.Converters.Add(converter))
            .Configure<JsonSerializerOptions, ExceptionJsonConverter>((options, converter) => options.Converters.Add(converter))
            .ConfigureSerializer(configure);

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
    }
}