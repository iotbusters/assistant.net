using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Service collection extensions for remote WEB message handling on a server.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds system services with self-hosted service based behavior.
        /// </summary>
        public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
            .AddSystemServicesDefaulted()
            .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        /// <summary>
        ///     Registers diagnostic services with self-hosted WEB service based behavior.
        /// </summary>
        public static IServiceCollection AddDiagnosticsWebHosted(this IServiceCollection services) => services
            .AddHttpContextAccessor()
            .AddDiagnosticContext(InitializeFromHttpContext)
            .AddDiagnostics();

        /// <summary>
        ///     Registers remote message handling server configuration customized by <paramref name="configure" />.
        /// </summary>
        public static IServiceCollection AddRemoteWebMessageHandler(this IServiceCollection services, Action<MessagingClientBuilder> configure) => services
            .AddSystemServicesHosted()
            .AddDiagnosticsWebHosted()
            .AddJsonSerialization()
            .AddTypeEncoder()
            .AddStorage(b => b.AddLocal<string, DeferredCachingResult>())
            .AddMessagingClient()
            .ConfigureMessageClient(b => b.AddConfiguration<ServerInterceptorConfiguration>())
            .ConfigureMessageClient(configure);

        /// <exception cref="InvalidOperationException" />
        private static string InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.GetCorrelationId();
        }
    }
}