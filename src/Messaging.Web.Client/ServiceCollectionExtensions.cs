using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        private static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);

        /// <summary>
        ///     Registers empty remote command handling configuration.
        ///     Assuming <see cref="RemoteWebCommandClientOptions" /> is configured separately.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebCommandClient(this IServiceCollection services)
        {
            return services
                .AddSystemLifetime()
                .AddDiagnostics()
                .AddTypeEncoder()
                .AddJsonSerialization()
                .AddHttpClient<IRemoteCommandClient, RemoteWebCommandClient>((p, c) =>
                {
                    var options = p.GetRequiredService<IOptions<RemoteWebCommandClientOptions>>().Value;
                    c.BaseAddress = options.BaseAddress;
                    c.Timeout = options.Timeout ?? DefaultTimeout;
                })
                .AddHttpMessageHandler<CorrelationHandler>()
                .AddHttpMessageHandler<OperationHandler>()
                .AddHttpMessageHandler<AuthorizationHandler>()
                .AddHttpMessageHandler<ErrorPropagationHandler>();
        }

        /// <summary>
        ///     Registers <see cref="IConfigurationSection" /> based remote command handling configuration.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebCommandClient(this IServiceCollection services, IConfigurationSection configuration) => services
            .AddRemoteWebCommandClientOptions(configuration)
            .AddRemoteWebCommandClient();

        /// <summary>
        ///     Registers remote command handling configuration customized by <paramref name="configureOptions" />.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebCommandClient(this IServiceCollection services, Action<RemoteWebCommandClientOptions> configureOptions) => services
            .AddRemoteWebCommandClientOptions(configureOptions)
            .AddRemoteWebCommandClient();

        /// <summary>
        ///     Registers a configuration instance which <see cref="RemoteWebCommandClientOptions"/> will bind against.
        /// </summary>
        internal static IServiceCollection AddRemoteWebCommandClientOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<RemoteWebCommandClientOptions>(configuration);

        /// <summary>
        ///     Registers an action used to configure <see cref="RemoteWebCommandClientOptions"/> options.
        /// </summary>
        internal static IServiceCollection AddRemoteWebCommandClientOptions(this IServiceCollection services, Action<RemoteWebCommandClientOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}