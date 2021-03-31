using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Web.Client;
using Assistant.Net.Messaging.Web.Serialization;

namespace Assistant.Net.Messaging.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoteCommandHandling(this IServiceCollection services, IConfigurationSection configuration)
        {
            services
                .AddJsonSerializerOptions()
                .Configure<RemoteCommandHandlingOptions>(configuration)
                .AddHttpClient<RemoteCommandHandlingClient>((p, c) =>
                {
                    var options = p.GetRequiredService<IOptions<RemoteCommandHandlingOptions>>().Value;
                    c.BaseAddress = options.Endpoint;
                    if (options.Timeout != null)
                        c.Timeout = options.Timeout.Value;
                })
                .AddHttpMessageHandler<MetricsHandler>()
                .AddHttpMessageHandler<AuthorizationHandler>()
                .AddHttpMessageHandler<ErrorPropagationHandler>();
            return services;
        }

        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services) =>
            services.AddJsonSerializerOptions(delegate { });

        public static IServiceCollection AddJsonSerializerOptions(this IServiceCollection services, Action<JsonSerializerOptions> configureOptions)
        {
            return services
                .AddScoped<ExceptionJsonConverter>()
                .Configure<JsonSerializerOptions>((p, o) =>
                {
                    o.Converters.Add(p.GetRequiredService<ExceptionJsonConverter>());
                    o.PropertyNameCaseInsensitive = true;
                    o.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                })
                .Configure(configureOptions);
        }


    }
}