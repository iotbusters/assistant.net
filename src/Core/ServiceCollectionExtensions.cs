using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Assistant.Net.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemClock(this IServiceCollection services, Func<IServiceProvider, DateTimeOffset> getUtcNow)
        {
            services.Replace(ServiceDescriptor.Singleton<ISystemClock>(p => new SystemClock(() => getUtcNow(p))));
            return services;
        }

        public static IServiceCollection AddSystemLifetime(this IServiceCollection services, Func<IServiceProvider, CancellationToken> getStoppingToken)
        {
            services.Replace(ServiceDescriptor.Singleton(p => new SystemLifetime(getStoppingToken(p))));
            services.TryAddTransient<ISystemLifetime>(p => p.GetRequiredService<SystemLifetime>());
            return services;
        }
    }
}