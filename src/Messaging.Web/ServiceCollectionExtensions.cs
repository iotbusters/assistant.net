using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers default configuration for <see cref="JsonSerializer" />.
        /// </summary>
        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services) => services
            .AddJsonSerializerOptions(delegate { });

        /// <summary>
        ///     Registers default configuration for <see cref="JsonSerializer" /> customized by <paramref name="configureOptions" /> predicate.
        /// </summary>
        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions) => services
            .AddScoped<CommandExceptionJsonConverter>()
            .Configure<JsonSerializerOptions, CommandExceptionJsonConverter>((options, converter) =>
             {
                 options.Converters.Add(converter);
                 options.PropertyNameCaseInsensitive = true;
                 options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                 options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
             })
            .Configure(configureOptions);
    }
}