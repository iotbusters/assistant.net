using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Web.Serialization;
using Assistant.Net.Messaging.Web.Options;
using Microsoft.Extensions.Configuration;

namespace Assistant.Net.Messaging.Web
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
            .Configure<RemoteCommandHandlingOptions>(configuration);

        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .Configure(configureOptions);

    }
}