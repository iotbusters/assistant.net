using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Web.Options;
using Assistant.Net.Messaging.Web.Client.Internal;
using Assistant.Net.Messaging.Web.Client.Extensions;

namespace Assistant.Net.Messaging.Web.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services) => services
            .AddJsonSerializerOptions()
            .AddHttpClient<RemoteCommandHandlingClient>((p, c) =>
            {
                var options = p.GetRequiredService<IOptions<RemoteCommandHandlingOptions>>().Value;
                c.BaseAddress = options.Endpoint;
                if (options.Timeout != null)
                    c.Timeout = options.Timeout.Value;
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