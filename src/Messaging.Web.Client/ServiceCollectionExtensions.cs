using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Service collection extensions for remote WEB messaging handling client.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);

        /// <summary>
        ///     Registers empty remote messaging handling configuration.
        ///     Assuming <see cref="RemoteWebMessagingClientOptions" /> is configured separately.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebMessagingClient(this IServiceCollection services)
        {
            return services
                .AddSystemLifetime()
                .AddDiagnostics()
                .AddTypeEncoder()
                .AddJsonSerialization()
                .AddHttpClient<IRemoteMessagingClient, RemoteWebMessagingClient>((p, c) =>
                {
                    var options = p.GetRequiredService<IOptions<RemoteWebMessagingClientOptions>>().Value;
                    c.BaseAddress = options.BaseAddress;
                    c.Timeout = options.Timeout ?? DefaultTimeout;
                })
                .AddHttpMessageHandler<CorrelationHandler>()
                .AddHttpMessageHandler<OperationHandler>()
                .AddHttpMessageHandler<AuthorizationHandler>()
                .AddHttpMessageHandler<ErrorPropagationHandler>();
        }

        /// <summary>
        ///     Registers <see cref="IConfigurationSection" /> based remote messaging handling configuration.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebMessagingClient(this IServiceCollection services, IConfigurationSection configuration) => services
            .AddRemoteWebMessagingClientOptions(configuration)
            .AddRemoteWebMessagingClient();

        /// <summary>
        ///     Registers remote messaging handling configuration customized by <paramref name="configureOptions" />.
        /// </summary>
        public static IHttpClientBuilder AddRemoteWebMessagingClient(this IServiceCollection services, Action<RemoteWebMessagingClientOptions> configureOptions) => services
            .AddRemoteWebMessagingClientOptions(configureOptions)
            .AddRemoteWebMessagingClient();

        /// <summary>
        ///     Registers a configuration instance which <see cref="RemoteWebMessagingClientOptions"/> will bind against.
        /// </summary>
        internal static IServiceCollection AddRemoteWebMessagingClientOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<RemoteWebMessagingClientOptions>(configuration);

        /// <summary>
        ///     Registers an action used to configure <see cref="RemoteWebMessagingClientOptions"/> options.
        /// </summary>
        internal static IServiceCollection AddRemoteWebMessagingClientOptions(this IServiceCollection services, Action<RemoteWebMessagingClientOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}