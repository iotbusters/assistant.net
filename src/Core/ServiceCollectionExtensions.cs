using Assistant.Net.Abstractions;
using Assistant.Net.Dynamics;
using Assistant.Net.Dynamics.Abstractions;
using Assistant.Net.Internal;
using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;

namespace Assistant.Net;

/// <summary>
///     Service collection extensions for core services and tools.
/// </summary>
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
    ///     Adds system services with self-hosted service based behavior.
    /// </summary>
    public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
        .AddSystemServicesDefaulted()
        .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

    /// <summary>
    ///     Adds <see cref="ISystemClock"/> implementation with default behavior
    ///     if it hasn't been already registered.
    /// </summary>
    public static IServiceCollection AddSystemClock(this IServiceCollection services) =>
        services.TryAddSingleton<ISystemClock>(_ => new SystemClock(() => DateTimeOffset.UtcNow));

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
        services.TryAddSingleton<ISystemLifetime>(_ => new SystemLifetime(CancellationToken.None));

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
        .AddLogging()
        .TryAddSingleton<ITypeEncoder, TypeEncoder>();

    /// <summary>
    ///     Adds <see cref="ITypeEncoder"/> implementation with default behavior
    ///     if it hasn't been already registered.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection AddTypeEncoder(this IServiceCollection services, Action<TypeEncoderOptions> configureOptions) => services
        .TryAddSingleton<ITypeEncoder, TypeEncoder>()
        .Configure(configureOptions);

    /// <summary>
    ///     Adds default <see cref="NamedOptionsContext"/>.
    /// </summary>
    public static IServiceCollection AddNamedOptionsContext(this IServiceCollection services) => services
        .TryAddScoped<NamedOptionsContext>()
        .TryAddScoped(typeof(INamedOptions<>), typeof(NamedOptions<>));

    /// <summary>
    ///     Gets a custom options builder that forwards Configure calls for the same <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    public static Options.OptionsBuilder<TOptions> AddOptions<TOptions>(this IServiceCollection services)
        where TOptions : class
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions();
        return new Options.OptionsBuilder<TOptions>(services, Microsoft.Extensions.Options.Options.DefaultName);
    }

    /// <summary>
    ///     Gets a custom options builder that forwards Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    public static Options.OptionsBuilder<TOptions> AddOptions<TOptions>(this IServiceCollection services, string name)
        where TOptions : class
    {
        services
            .AddOptions()
            .TryAddScoped(typeof(IOptionsSnapshotCache<>), typeof(OptionsSnapshotCache<>))
            .ReplaceScoped(typeof(IOptionsSnapshot<>), typeof(Options.OptionsManager<>));
        return new Options.OptionsBuilder<TOptions>(services, name);
    }

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services"/>
    /// <param name="source">Custom configuration options source instance.</param>
    public static IServiceCollection BindOptions<TOptions>(this IServiceCollection services, IConfigureOptionsSource<TOptions> source)
        where TOptions : class => services
        .BindOptions(Microsoft.Extensions.Options.Options.DefaultName, source);

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="source">Custom configuration options source instance.</param>
    public static IServiceCollection BindOptions<TOptions>(this IServiceCollection services, string name, IConfigureOptionsSource<TOptions> source)
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Bind(source)
        .Services;

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services"/>
    /// <param name="factory">Custom configuration options source instance factory.</param>
    public static IServiceCollection BindOptions<TOptions>(this IServiceCollection services, Func<IServiceProvider, IConfigureOptionsSource<TOptions>> factory)
        where TOptions : class => services
        .BindOptions(Microsoft.Extensions.Options.Options.DefaultName, factory);

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="factory">Custom configuration options source instance factory.</param>
    public static IServiceCollection BindOptions<TOptions>(this IServiceCollection services, string name, Func<IServiceProvider, IConfigureOptionsSource<TOptions>> factory)
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Bind(factory)
        .Services;

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TConfigureOptionsSource">Custom configuration options source type.</typeparam>
    public static IServiceCollection BindOptions<TOptions, TConfigureOptionsSource>(this IServiceCollection services)
        where TConfigureOptionsSource : IConfigureOptionsSource<TOptions>
        where TOptions : class => services
        .BindOptions<TOptions, TConfigureOptionsSource>(Microsoft.Extensions.Options.Options.DefaultName);

    /// <summary>
    ///     Registers Configure calls for the same named <typeparamref name="TOptions"/>
    ///     to the underlying service collection and custom options binding configuration.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <typeparam name="TConfigureOptionsSource">Custom configuration options source type.</typeparam>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    public static IServiceCollection BindOptions<TOptions, TConfigureOptionsSource>(this IServiceCollection services, string name)
        where TConfigureOptionsSource : IConfigureOptionsSource<TOptions>
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Bind<TConfigureOptionsSource>()
        .Services;

    /// <summary>
    ///     Registers a configuration instance which TOptions will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="config">The configuration being bound.</param>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, IConfigurationSection config)
        where TOptions : class => services
        .Configure<TOptions>(Microsoft.Extensions.Options.Options.DefaultName, config);

    /// <summary>
    ///     Registers a configuration instance which TOptions will bind against.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="config">The configuration being bound.</param>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, IConfigurationSection config)
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Bind(config)
        .ValidateDataAnnotations()
        .Services;

    /// <summary>
    ///     Registers an options source instance which TOptions will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="source">The options source being bound.</param>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, IConfigureOptionsSource<TOptions> source)
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Bind(source)
        .ValidateDataAnnotations()
        .Services;

    /// <summary>
    ///     Registers an action used to configure a particular type of options with following validation.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
        where TOptions : class => services
        .Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers an action used to configure a particular type of options with following validation.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection Configure<TOptions>(this IServiceCollection services, string name, Action<TOptions> configureOptions)
        where TOptions : class => services
        .AddOptions<TOptions>(name)
        .Configure(configureOptions)
        .ValidateDataAnnotations()
        .Services;

    /// <summary>
    ///     Registers an action used to configure a particular type of options with following validation.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection Configure<TOptions, TDep>(this IServiceCollection services, Action<TOptions, TDep> configureOptions)
        where TOptions : class
        where TDep : class => services
        .Configure(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers an action used to configure a particular type of options with following validation.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection Configure<TOptions, TDep>(this IServiceCollection services, string name, Action<TOptions, TDep> configureOptions)
        where TOptions : class
        where TDep : class => services
        .AddOptions<TOptions>(name)
        .Configure(configureOptions)
        .ValidateDataAnnotations()
        .Services;

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Transient" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddTransientEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddTransient(IServiceCollection,Type,Type)" /> is not idempotent and
    ///     it adds duplicate <paramref name="serviceType"/> and its <paramref name="implementationType"/> if called twice.
    ///     Using <see cref="TryAddTransient(IServiceCollection,Type,Type)" /> is idempotent but
    ///     it ignores a <paramref name="serviceType" /> based implementation if called twice.
    ///     Using <see cref="TryAddTransientEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <param name="services" />
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddTransientEnumerable(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient(serviceType, implementationType));
        return services;
    }

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Transient" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddTransientEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddTransient{TService,TImplementation}(IServiceCollection)" /> is not idempotent and
    ///     it adds duplicate <typeparamref name="TService"/> type and its <typeparamref name="TImplementation"/> type if called twice.
    ///     Using <see cref="TryAddTransient{TService,TImplementation}(IServiceCollection)" /> is idempotent but
    ///     it ignores a <typeparamref name="TService"/> type based implementation if called twice.
    ///     Using <see cref="TryAddTransientEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddTransientEnumerable<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class => services
        .TryAddTransientEnumerable(typeof(TService), typeof(TImplementation));

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Scoped" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddScopedEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddScoped(IServiceCollection,Type,Type)" /> is not idempotent and
    ///     it adds duplicate <paramref name="serviceType"/> and its <paramref name="implementationType"/> if called twice.
    ///     Using <see cref="TryAddScoped(IServiceCollection,Type,Type)" /> is idempotent but
    ///     it ignores a <paramref name="serviceType" /> based implementation if called twice.
    ///     Using <see cref="TryAddScopedEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddScopedEnumerable(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped(serviceType, implementationType));
        return services;
    }

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Scoped" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddScopedEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddScoped{TService,TImplementation}(IServiceCollection)" /> is not idempotent and
    ///     it adds duplicate <typeparamref name="TService"/> type and its <typeparamref name="TImplementation"/> type if called twice.
    ///     Using <see cref="TryAddScoped{TService,TImplementation}(IServiceCollection)" /> is idempotent but
    ///     it ignores a <typeparamref name="TService"/> type based implementation if called twice.
    ///     Using <see cref="TryAddScopedEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddScopedEnumerable<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class => services
        .TryAddScopedEnumerable(typeof(TService), typeof(TImplementation));

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Singleton" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddTransientEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddSingleton(IServiceCollection,Type,Type)" /> is not idempotent and
    ///     it adds duplicate <paramref name="serviceType"/> and its <paramref name="implementationType"/> if called twice.
    ///     Using <see cref="TryAddSingleton(IServiceCollection,Type,Type)" /> is idempotent but
    ///     it ignores a <paramref name="serviceType" /> based implementation if called twice.
    ///     Using <see cref="TryAddSingletonEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddSingletonEnumerable(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton(serviceType, implementationType));
        return services;
    }

    /// <summary>
    ///     Adds a <see cref="ServiceLifetime.Singleton" /> descriptor if an existing one with the same service and its implementation
    ///     that does not already exist in <paramref name="services." />.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="TryAddSingletonEnumerable" /> when registering a service implementation of a service type
    ///     that supports multiple registrations of the same service type.
    ///     Using <see cref="ServiceCollectionServiceExtensions.AddSingleton{TService,TImplementation}(IServiceCollection)" /> is not idempotent and
    ///     it adds duplicate <typeparamref name="TService"/> type and its <typeparamref name="TImplementation"/> type if called twice.
    ///     Using <see cref="TryAddSingleton{TService,TImplementation}(IServiceCollection)" /> is idempotent but
    ///     it ignores a <typeparamref name="TService"/> type based implementation if called twice.
    ///     Using <see cref="TryAddSingletonEnumerable" /> will prevent only registration of multiple implementation types.
    /// </remarks>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddSingletonEnumerable<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class => services
        .TryAddSingletonEnumerable(typeof(TService), typeof(TImplementation));

    /// <summary>
    ///     Adds the specified <paramref name="serviceType"/> as a <see cref="ServiceLifetime.Transient"/> service
    ///     with the <paramref name="implementationType"/> implementation
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        ServiceCollectionDescriptorExtensions.TryAddTransient(services, serviceType, implementationType);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Transient"/> service
    ///     implementation type specified in <typeparamref name="TImplementation"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddTransient<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddTransient<TService, TImplementation>(services);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Transient"/> service
    ///     using the factory specified in <paramref name="implementationFactory"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection TryAddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddTransient(services, implementationFactory);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Transient"/> service
    ///     implementation type specified in <typeparamref name="TService"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    public static IServiceCollection TryAddTransient<TService>(this IServiceCollection services)
        where TService : class =>
        services.TryAddTransient<TService, TService>();

    /// <summary>
    ///     Adds the specified <paramref name="serviceType"/> as a <see cref="ServiceLifetime.Scoped"/> service
    ///     with the <paramref name="implementationType"/> implementation
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddScoped(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        ServiceCollectionDescriptorExtensions.TryAddScoped(services, serviceType, implementationType);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Scoped"/> service
    ///     implementation type specified in <typeparamref name="TImplementation"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddScoped<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddScoped<TService, TImplementation>(services);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Scoped"/> service
    ///     using the factory specified in <paramref name="implementationFactory"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection TryAddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddScoped(services, implementationFactory);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Scoped"/> service
    ///     implementation type specified in <typeparamref name="TService"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    public static IServiceCollection TryAddScoped<TService>(this IServiceCollection services)
        where TService : class =>
        services.TryAddScoped<TService, TService>();

    /// <summary>
    ///     Adds the specified <paramref name="serviceType"/> as a <see cref="ServiceLifetime.Singleton"/> service
    ///     with the <paramref name="implementationType"/> implementation
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection TryAddSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        ServiceCollectionDescriptorExtensions.TryAddSingleton(services, serviceType, implementationType);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Singleton"/> service
    ///     using the factory specified in <paramref name="implementationFactory"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection TryAddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddSingleton(services, implementationFactory);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Singleton"/> service
    ///     implementation type specified in <typeparamref name="TImplementation"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    public static IServiceCollection TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class
    {
        ServiceCollectionDescriptorExtensions.TryAddSingleton<TService, TImplementation>(services);
        return services;
    }

    /// <summary>
    ///     Adds the specified <typeparamref name="TService"/> as a <see cref="ServiceLifetime.Singleton"/> service
    ///     implementation type specified in <typeparamref name="TService"/>
    ///     to the <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    public static IServiceCollection TryAddSingleton<TService>(this IServiceCollection services)
        where TService : class =>
        services.TryAddSingleton<TService, TService>();

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Transient"/> service implementation type
    ///     specified in <typeparamref name="TImplementation"/>.
    /// </summary>
    public static IServiceCollection ReplaceTransient<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class =>
        services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Transient"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class =>
        services.Replace(ServiceDescriptor.Transient(implementationFactory));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Transient"/> service implementation type
    ///     specified in <typeparamref name="TService"/>.
    /// </summary>
    public static IServiceCollection ReplaceTransient<TService>(this IServiceCollection services)
        where TService : class =>
        services.ReplaceTransient<TService, TService>();

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Transient"/> with the <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection ReplaceTransient(this IServiceCollection services, Type serviceType, Type implementationType) =>
        services.Replace(ServiceDescriptor.Transient(serviceType, implementationType));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Transient"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory) =>
        services.Replace(ServiceDescriptor.Transient(serviceType, implementationFactory));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Scoped"/> service implementation type
    ///     specified in <typeparamref name="TImplementation"/>.
    /// </summary>
    public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class =>
        services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Scoped"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class =>
        services.Replace(ServiceDescriptor.Scoped(implementationFactory));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Scoped"/> service implementation type
    ///     specified in <typeparamref name="TService"/>.
    /// </summary>
    public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services)
        where TService : class =>
        services.ReplaceScoped<TService, TService>();

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Scoped"/> with the <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection ReplaceScoped(this IServiceCollection services, Type serviceType, Type implementationType) =>
        services.Replace(ServiceDescriptor.Scoped(serviceType, implementationType));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Scoped"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory) =>
        services.Replace(ServiceDescriptor.Scoped(serviceType, implementationFactory));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Singleton"/> service implementation type
    ///     specified in <typeparamref name="TImplementation"/>.
    /// </summary>
    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TService
        where TService : class =>
        services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Singleton"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
        where TService : class =>
        services.Replace(ServiceDescriptor.Singleton(implementationFactory));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <typeparamref name="TService"/>
    ///     and adds it as a <see cref="ServiceLifetime.Singleton"/> service implementation type
    ///     specified in <typeparamref name="TService"/>.
    /// </summary>
    public static IServiceCollection ReplaceSingleton<TService>(this IServiceCollection services)
        where TService : class =>
        services.ReplaceSingleton<TService, TService>();

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Singleton"/> with the <paramref name="implementationType"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationType">The implementation type of the service.</param>
    public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, Type implementationType) =>
        services.Replace(ServiceDescriptor.Singleton(serviceType, implementationType));

    /// <summary>
    ///     Removes the first service in <see cref="IServiceCollection"/> with the same <paramref name="serviceType"/>
    ///     and adds it as a <see cref="ServiceLifetime.Singleton"/> service using the factory
    ///     specified in <paramref name="implementationFactory"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="serviceType">The type of the service to register.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory) =>
        services.Replace(ServiceDescriptor.Singleton(serviceType, implementationFactory));

    /// <summary>
    ///     Decorates the registered <typeparamref name="TService"/> to the <paramref name="services"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, a proxy will be generated during configuration time so it will take additional time.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="configureProxy">The action used to decorate the proxy.</param>
    /// <exception cref="ArgumentException"/>
    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Action<Proxy<TService>> configureProxy)
        where TService : class
    {
        var existingDescriptor = services.Reverse().FirstOrDefault(x => x.ServiceType == typeof(TService)) ??
                                 throw new ArgumentException($"Service '{typeof(TService).Name}' wasn't registered.");

        var updatedDescriptor = existingDescriptor switch
        {
            {ImplementationType: var it, ImplementationFactory: null, ImplementationInstance: null} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(it!, configureProxy), existingDescriptor.Lifetime),
            { ImplementationType: null, ImplementationFactory: var f, ImplementationInstance: null} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(f, configureProxy), existingDescriptor.Lifetime),
            {ImplementationType: null, ImplementationFactory: null, ImplementationInstance: var ii} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(ii!, configureProxy), existingDescriptor.Lifetime),
            _ => throw new NotSupportedException("Unexpected service descriptor.")
        };
        services.Add(updatedDescriptor);

        return services.AddProxyFactory(o => o.Add<TService>());
    }

    /// <summary>
    ///     Decorates the registered <typeparamref name="TService"/> to the <paramref name="services"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, a proxy will be generated during configuration time so it will take additional time.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="configureProxy">The action used to decorate the proxy.</param>
    /// <exception cref="ArgumentException"/>
    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Action<IServiceProvider, Proxy<TService>> configureProxy)
        where TService : class
    {
        var existingDescriptor = services.Reverse().FirstOrDefault(x => x.ServiceType == typeof(TService)) ??
                                 throw new ArgumentException($"Service '{typeof(TService).Name}' wasn't registered.");

        var updatedDescriptor = existingDescriptor switch
        {
            {ImplementationType: var it, ImplementationFactory: null, ImplementationInstance: null} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(it!, configureProxy), existingDescriptor.Lifetime),
            { ImplementationType: null, ImplementationFactory: var f, ImplementationInstance: null} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(f, configureProxy), existingDescriptor.Lifetime),
            {ImplementationType: null, ImplementationFactory: null, ImplementationInstance: var ii} =>
                new ServiceDescriptor(existingDescriptor.ServiceType, DecoratingFactory(ii!, configureProxy), existingDescriptor.Lifetime),
            _ => throw new NotSupportedException("Unexpected service descriptor.")
        };
        services.Add(updatedDescriptor);

        return services.AddProxyFactory(o => o.Add<TService>());
    }

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        Type implementationType,
        Action<Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)ActivatorUtilities.CreateInstance(p, implementationType));
        configureProxy(proxy);
        return proxy.Object;
    };

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        Func<IServiceProvider, object> implementationFactory,
        Action<Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)implementationFactory(p));
        configureProxy(proxy);
        return proxy.Object;
    };

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        object implementationInstance,
        Action<Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)implementationInstance);
        configureProxy(proxy);
        return proxy.Object;
    };

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        Type implementationType,
        Action<IServiceProvider, Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)ActivatorUtilities.CreateInstance(p, implementationType));
        configureProxy(p, proxy);
        return proxy.Object;
    };

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        Func<IServiceProvider, object> implementationFactory,
        Action<IServiceProvider, Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)implementationFactory(p));
        configureProxy(p, proxy);
        return proxy.Object;
    };

    private static Func<IServiceProvider, TService> DecoratingFactory<TService>(
        object implementationInstance,
        Action<IServiceProvider, Proxy<TService>> configureProxy)
        where TService : class => p =>
    {
        var proxy = p.GetRequiredService<IProxyFactory>().Create((TService)implementationInstance);
        configureProxy(p, proxy);
        return proxy.Object;
    };

    /// <summary>
    ///     Register this options instance for validation of its DataAnnotations.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    private static Options.OptionsBuilder<TOptions> ValidateDataAnnotations<TOptions>(this Options.OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        if (optionsBuilder.Services.Any(x =>
                x.ImplementationInstance is DataAnnotationValidateOptions<TOptions> builder
                && builder.Name == optionsBuilder.Name))
            return optionsBuilder;

        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(new DataAnnotationValidateOptions<TOptions>(optionsBuilder.Name));
        return optionsBuilder;
    }

    /// <summary>
    ///     Register this options instance for validation of its DataAnnotations.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    private static Microsoft.Extensions.Options.OptionsBuilder<TOptions> ValidateDataAnnotations<TOptions>(this Microsoft.Extensions.Options.OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        if (optionsBuilder.Services.Any(x =>
                x.ImplementationInstance is DataAnnotationValidateOptions<TOptions> builder
                && builder.Name == optionsBuilder.Name))
            return optionsBuilder;

        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(new DataAnnotationValidateOptions<TOptions>(optionsBuilder.Name));
        return optionsBuilder;
    }
}
