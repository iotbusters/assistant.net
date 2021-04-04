using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services) => services
            .AddJsonSerializerOptions(delegate { });

        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions) => services
            .AddScoped<ExceptionJsonConverter>()
            .Configure<JsonSerializerOptions, ExceptionJsonConverter>((options, converter) =>
             {
                 options.Converters.Add(converter);
                 options.PropertyNameCaseInsensitive = true;
                 options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                 options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
             })
            .Configure(configureOptions);
    }
}