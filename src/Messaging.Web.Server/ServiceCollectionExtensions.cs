using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Configuration;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds system services with self-hosted service based behavior.
        /// </summary>
        public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
            .AddSystemServicesDefaulted()
            .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        /// <summary>
        ///     Registers remote command handling server configuration customized by <paramref name="configure" />.
        /// </summary>
        public static IServiceCollection AddRemoteWebCommandHandler(this IServiceCollection services, Action<CommandClientBuilder> configure) => services
            .AddStorage(b => b.AddLocal<DeferredCachingResult>())
            .AddTypeEncoder()
            .AddDiagnostics()
            .AddDiagnosticContext(InitializeFromHttpContext)
            .AddHttpContextAccessor()
            .AddJsonSerializerOptions()
            .AddCommandClient()
            .ConfigureCommandClient(b => b.AddConfiguration<ServerInterceptorConfiguration>())
            .ConfigureCommandClient(configure);

        private static string InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.GetCorrelationId();
        }
    }
}