using System;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Assistant.Net.Abstractions;
using Assistant.Net.Internal;
using Assistant.Net.Options;

namespace Assistant.Net
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

        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<IServiceProvider, TOptions> configureOptions)
            where TOptions : class =>
            services.Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, Action<IServiceProvider, TOptions> configureOptions)
            where TOptions : class
        {

            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.AddOptions();
            return services.AddSingleton<IConfigureNamedOptions<TOptions>>(p => new InjectableConfigureNamedOptions<TOptions>(name, p, configureOptions));
        }
    }
}