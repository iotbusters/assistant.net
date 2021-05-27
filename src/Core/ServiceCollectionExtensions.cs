using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Assistant.Net.Abstractions;
using Assistant.Net.Internal;

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
        public static IServiceCollection AddSystemClock(this IServiceCollection services) =>
            services.TryAddSingleton<ISystemClock>(p => new SystemClock(() => DateTimeOffset.UtcNow));

        /// <summary>
        ///     Adds or replaces <see cref="ISystemClock"/> implementation with configured behavior.
        /// </summary>
        public static IServiceCollection AddSystemClock(this IServiceCollection services, Func<IServiceProvider, DateTimeOffset> getUtcNow) => services
            .ReplaceSingleton<ISystemClock>(p => new SystemClock(() => getUtcNow(p)));

        /// <summary>
        ///     Adds <see cref="ISystemLifetime"/> implementation with default behavior
        ///     if it hasn't been already registered.
        /// </summary>
        public static IServiceCollection AddSystemLifetime(this IServiceCollection services) =>
            services.TryAddSingleton<ISystemLifetime>(p => new SystemLifetime(CancellationToken.None));

        /// <summary>
        ///     Adds or replaces <see cref="ISystemLifetime"/> implementation with configured behavior.
        /// </summary>
        public static IServiceCollection AddSystemLifetime(this IServiceCollection services, Func<IServiceProvider, CancellationToken> getStoppingToken) => services
            .ReplaceSingleton<ISystemLifetime>(p => new SystemLifetime(getStoppingToken(p)));

        /// <summary>
        ///     Adds <see cref="ITypeEncoder"/> implementation with default behavior
        ///     if it hasn't been already registered.
        /// </summary>
        public static IServiceCollection AddTypeEncoder(this IServiceCollection services) => services
            .TryAddSingleton<ITypeEncoder, TypeEncoder>();

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
        public static IServiceCollection Configure<TOptions, TDep>(this IServiceCollection services, Action<TOptions, TDep> configureOptions)
            where TOptions : class
            where TDep : class => services
            .Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        /// <summary>
        ///     Registers an action used to configure a particular type of options with following validation.
        /// </summary>
        public static IServiceCollection Configure<TOptions, TDep>(this IServiceCollection services, string name, Action<TOptions, TDep> configureOptions)
            where TOptions : class
            where TDep : class => services
            .AddOptions<TOptions>(name)
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .Services;

        public static IServiceCollection TryAddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            ServiceCollectionDescriptorExtensions.TryAddTransient(services, serviceType, implementationType);
            return services;
        }

        public static IServiceCollection TryAddTransient<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddTransient<TService, TImplementation>(services);
            return services;
        }

        public static IServiceCollection TryAddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddTransient(services, implementationFactory);
            return services;
        }

        public static IServiceCollection TryAddTransient<TService>(this IServiceCollection services)
            where TService : class =>
            services.TryAddTransient<TService, TService>();

        public static IServiceCollection TryAddScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            ServiceCollectionDescriptorExtensions.TryAddScoped(services, serviceType, implementationType);
            return services;
        }

        public static IServiceCollection TryAddScoped<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddScoped<TService, TImplementation>(services);
            return services;
        }

        public static IServiceCollection TryAddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddScoped(services, implementationFactory);
            return services;
        }

        public static IServiceCollection TryAddScoped<TService>(this IServiceCollection services)
            where TService : class =>
            services.TryAddScoped<TService, TService>();

        public static IServiceCollection TryAddSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            ServiceCollectionDescriptorExtensions.TryAddSingleton(services, serviceType, implementationType);
            return services;
        }

        public static IServiceCollection TryAddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddSingleton(services, implementationFactory);
            return services;
        }

        public static IServiceCollection TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class
        {
            ServiceCollectionDescriptorExtensions.TryAddSingleton<TService, TImplementation>(services);
            return services;
        }

        public static IServiceCollection TryAddSingleton<TService>(this IServiceCollection services)
            where TService : class =>
            services.TryAddSingleton<TService, TService>();

        public static IServiceCollection ReplaceTransient<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class =>
            services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());

        public static IServiceCollection ReplaceTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class =>
            services.Replace(ServiceDescriptor.Transient(implementationFactory));

        public static IServiceCollection ReplaceTransient<TService>(this IServiceCollection services)
            where TService : class =>
            services.ReplaceTransient<TService, TService>();

        public static IServiceCollection ReplaceTransient(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Replace(ServiceDescriptor.Transient(serviceType, implementationType));

        public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class =>
            services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());

        public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class =>
            services.Replace(ServiceDescriptor.Scoped(implementationFactory));

        public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services)
            where TService : class =>
            services.ReplaceScoped<TService, TService>();

        public static IServiceCollection ReplaceScoped(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Replace(ServiceDescriptor.Scoped(serviceType, implementationType));
        public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class =>
            services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());

        public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
            where TService : class =>
            services.Replace(ServiceDescriptor.Singleton(implementationFactory));

        public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services)
            where TService : class =>
            services.ReplaceSingleton<TService, TService>();

        public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Replace(ServiceDescriptor.Singleton(serviceType, implementationType));
    }
}