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
        ///     Registers remote command handling server configuration customized by <paramref name="configureOptions" />.
        /// </summary>
        public static IServiceCollection AddRemoteCommandHandlingServer(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddStorage(b => b.AddLocal<DeferredCachingResult>())
            .AddDiagnostics()
            .AddDiagnosticContext(InitializeFromHttpContext)
            .AddHttpContextAccessor()
            .AddJsonSerializerOptions()
            .AddCommandClient(configureOptions)
            .AddCommandOptions(opt => opt.Add<ServerInterceptorConfiguration>());

        private static string InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.GetCorrelationId();
        }
    }
}