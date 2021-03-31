using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Web.Client;

namespace Assistant.Net.Messaging.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoteCommandHandling(this IServiceCollection services, IConfigurationSection configuration)
        {
            services
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
    }
}