using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
            .AddSystemServicesDefaulted()
            .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        public static IServiceCollection AddRemoteCommandHandlingServer(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddDiagnostics()
            .AddDiagnosticsContext(InitializeFromHttpContext)
            .AddHttpContextAccessor()
            .AddJsonSerializerOptions()
            .AddCommandClient(configureOptions);

        private static Guid InitializeFromHttpContext(IServiceProvider provider)
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

            return context.GetCorrelationId();
        }
    }
}