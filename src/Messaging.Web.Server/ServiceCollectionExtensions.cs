using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
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
            .AddRemoteWebMessageHandlerMiddlewares()
            .AddSystemServicesHosted()
            .AddDiagnosticsWebHosted()
            .AddJsonSerialization()
            .AddMessagingClient()
            .ConfigureMessageClient(b => b.AddConfiguration<ServerInterceptorConfiguration>())
            .ConfigureMessageClient(configure);

        /// <summary>
        ///     Registers remote message handling middlewares:
        ///     <see cref="DiagnosticMiddleware"/>, <see cref="ExceptionHandlingMiddleware"/> and <see cref="MessageHandlingMiddleware"/>.
        /// </summary>
        public static IServiceCollection AddRemoteWebMessageHandlerMiddlewares(this IServiceCollection services) => services
            .TryAddTransient<DiagnosticMiddleware>()
            .TryAddTransient<ExceptionHandlingMiddleware>()
            .TryAddTransient<MessageHandlingMiddleware>();

        /// <exception cref="InvalidOperationException" />
        private static string InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.GetCorrelationId();
        }
    }
}
