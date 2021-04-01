using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Assistant.Net;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services) => services
            .AddJsonSerializerOptions(delegate { });

        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions) => services
            .AddScoped<ExceptionJsonConverter>()
            .Configure<JsonSerializerOptions>((p, o) =>
            {
                o.Converters.Add(p.GetRequiredService<ExceptionJsonConverter>());
                o.PropertyNameCaseInsensitive = true;
                o.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            })
            .Configure(configureOptions);

        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .AddOptions<RemoteCommandHandlingOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .Services;

        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .Configure(configureOptions);

    }
}