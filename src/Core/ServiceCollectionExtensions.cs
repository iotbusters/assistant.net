using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Assistant.Net.Abstractions;
using Assistant.Net.Internal;
using Microsoft.Extensions.Configuration;

namespace Assistant.Net
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds system services with default behavior.
        /// </summary>
        public static IServiceCollection AddSystemServicesDefaulted(this IServiceCollection services) => services
            .AddLogging()
            .AddSystemClock()
            .AddSystemLifetime();


        /// <summary>
        ///     Adds <see cref="ISystemClock"/> implementation with default behavior
        ///     if it hasn't been already registered.
        /// </summary>
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            services.TryAddSingleton<ISystemClock>(p => new SystemClock(() => DateTimeOffset.UtcNow));
            return services;
        }

        /// <summary>
        ///     Adds or replaces <see cref="ISystemClock"/> implementation with configured behavior.
        /// </summary>
        public static IServiceCollection AddSystemClock(this IServiceCollection services, Func<IServiceProvider, DateTimeOffset> getUtcNow) => services
            .Replace(ServiceDescriptor.Singleton<ISystemClock>(p => new SystemClock(() => getUtcNow(p))));

        /// <summary>
        ///     Adds <see cref="ISystemLifetime"/> implementation with default behavior
        ///     if it hasn't been already registered.
        /// </summary>
        public static IServiceCollection AddSystemLifetime(this IServiceCollection services)
        {
            services.TryAddSingleton<ISystemLifetime>(p => new SystemLifetime(CancellationToken.None));
            return services;
        }

        /// <summary>
        ///     Adds or replaces <see cref="ISystemLifetime"/> implementation with configured behavior.
        /// </summary>
        public static IServiceCollection AddSystemLifetime(this IServiceCollection services, Func<IServiceProvider, CancellationToken> getStoppingToken) => services
            .Replace(ServiceDescriptor.Singleton<ISystemLifetime>(p => new SystemLifetime(getStoppingToken(p))));

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfigurationSection config)
            where TOptions : class => services
            .Configure<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, config);

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, IConfigurationSection config)
            where TOptions : class => services
            .AddOptions<TOptions>(name)
            .Bind(config)
            .ValidateDataAnnotations()
            .Services;

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TOptions : class => services
            .Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, Action<TOptions> configureOptions)
            where TOptions : class => services
            .AddOptions<TOptions>(name)
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .Services;

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions,TDep>(this IServiceCollection services, Action<TOptions, TDep> configureOptions)
            where TOptions : class 
            where TDep : class => services
            .Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions,TDep>(this IServiceCollection services, string name, Action<TOptions,TDep> configureOptions)
            where TOptions : class 
            where TDep : class => services
            .AddOptions<TOptions>(name)
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .Services;
    }
}