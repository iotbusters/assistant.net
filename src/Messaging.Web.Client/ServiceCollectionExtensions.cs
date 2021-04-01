using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Extensions;
using System;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        private static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);

        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services) => services
            .AddJsonSerializerOptions()
            .AddHttpClient<RemoteCommandHandlingClient>((p, c) =>
            {
                var options = p.GetRequiredService<IOptions<RemoteCommandHandlingOptions>>().Value;
                c.BaseAddress = options.Endpoint;
                c.Timeout = options.Timeout ?? DefaultTimeout;
            })
            .AddHttpMessageHandler<MetricsHandler>()
            .AddHttpMessageHandler<AuthorizationHandler>()
            .AddHttpMessageHandler<ErrorPropagationHandler>()
            .Services;

        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services, IConfigurationSection configuration) => services
            .AddRemoteCommandHandlingOptions(configuration)
            .AddRemoteCommandHandlingClient();
    }
}