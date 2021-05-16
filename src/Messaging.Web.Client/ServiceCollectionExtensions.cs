using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Extensions;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        private static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);

        /// <summary>
        ///     Registers empty remote command handling configuration.
        ///     Assuming <see cref="RemoteCommandHandlingOptions" /> is configured separately.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services) => services
            .AddDiagnostics()
            .AddJsonSerializerOptions()
            .AddHttpClient<RemoteCommandHandlingClient>((p, c) =>
            {
                var options = p.GetRequiredService<IOptions<RemoteCommandHandlingOptions>>().Value;
                c.BaseAddress = options.BaseAddress;
                c.Timeout = options.Timeout ?? DefaultTimeout;
            })
            .AddHttpMessageHandler<CorrelationHandler>()
            .AddHttpMessageHandler<OperationHandler>()
            .AddHttpMessageHandler<AuthorizationHandler>()
            .AddHttpMessageHandler<ErrorPropagationHandler>()
            .Services;

        /// <summary>
        ///     Registers <see cref="IConfigurationSection" /> based remote command handling configuration.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services, IConfigurationSection config) => services
            .AddRemoteCommandHandlingOptions(config)
            .AddRemoteCommandHandlingClient();

        /// <summary>
        ///     Registers remote command handling configuration customized by <paramref name="configureOptions" />.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingClient(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .AddRemoteCommandHandlingOptions(configureOptions)
            .AddRemoteCommandHandlingClient();

        /// <summary>
        ///     Registers a configuration instance which <see cref="RemoteCommandHandlingOptions"/> will bind against.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<RemoteCommandHandlingOptions>(configuration);

        /// <summary>
        ///     Registers an action used to configure <see cref="RemoteCommandHandlingOptions"/> options.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingOptions(this IServiceCollection services, Action<RemoteCommandHandlingOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}