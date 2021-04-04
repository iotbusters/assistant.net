using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.CommandOptions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Extensions;

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
                c.BaseAddress = options.BaseAddress;
                c.Timeout = options.Timeout ?? DefaultTimeout;
            })
            .AddHttpMessageHandler<MetricsHandler>()
            .AddHttpMessageHandler<AuthorizationHandler>()
            .AddHttpMessageHandler<ErrorPropagationHandler>()
            .Services;

        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services, IConfigurationSection config) => services
            .AddRemoteCommandHandlingOptions(config)
            .AddRemoteCommandHandlingClient();

        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .AddRemoteCommandHandlingOptions(configureOptions)
            .AddRemoteCommandHandlingClient();

        /// <summary>
        ///     Registers a configuration instance which <see cref="RemoteCommandHandlingOptions"/> will bind against.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<RemoteCommandHandlingOptions>(configuration);

        /// <summary>
        ///     Register an action used to configure <see cref="RemoteCommandHandlingOptions"/> options.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}