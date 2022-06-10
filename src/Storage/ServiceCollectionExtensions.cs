using Assistant.Net.Diagnostics;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Converters;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Service collection extensions for storages.
/// </summary>
/// <remarks>
///     Pay attention, you need to call explicitly <see cref="ConfigureStorage(IServiceCollection,Action{StorageBuilder})"/> to register storing types.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds storage implementation, required services and defaults.
    /// </summary>
    public static IServiceCollection AddStorage(this IServiceCollection services) => services
        .AddLogging()
        .AddSystemClock()
        .AddDiagnostics()
        .AddTypeEncoder()
        .AddNamedOptionsContext()
        .ConfigureStorage(b => b.AddConfiguration<DefaultConverterConfiguration>())
        .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
        .TryAddScoped(typeof(IAdminStorage<,>), typeof(Storage<,>))
        .TryAddScoped(typeof(IHistoricalStorage<,>), typeof(HistoricalStorage<,>))
        .TryAddScoped(typeof(IHistoricalAdminStorage<,>), typeof(HistoricalStorage<,>))
        .TryAddScoped(typeof(IPartitionedStorage<,>), typeof(PartitionedStorage<,>))
        .TryAddScoped(typeof(IPartitionedAdminStorage<,>), typeof(PartitionedStorage<,>))
        .TryAddScoped(typeof(IValueConverter<>), typeof(TypedValueConverter<>));

    /// <summary>
    ///     Adds common services required by storage implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, all storing types should be previously registered.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure default option instances.</param>
    public static IServiceCollection AddStorage(this IServiceCollection services, Action<StorageBuilder> configure) => services
        .AddStorage()
        .AddSerializer()
        .ConfigureStorage(configure);

    /// <summary>
    ///     Adds common services required by storage implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, all storing types should be previously registered.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configure">The action used to configure option instances.</param>
    public static IServiceCollection AddStorage(this IServiceCollection services, string name, Action<StorageBuilder> configure) => services
        .AddStorage()
        .AddSerializer(name, delegate { })
        .ConfigureStorage(name, b => b.AddConfiguration<DefaultConverterConfiguration>())
        .ConfigureStorage(name, configure);

    /// <summary>
    ///     Configures storage implementations, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure option instances.</param>
    public static IServiceCollection ConfigureStorage(this IServiceCollection services, Action<StorageBuilder> configure) => services
        .ConfigureStorage(Microsoft.Extensions.Options.Options.DefaultName, configure);

    /// <summary>
    ///     Configures storage implementations, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configure">The action used to configure option instances.</param>
    public static IServiceCollection ConfigureStorage(this IServiceCollection services, string name, Action<StorageBuilder> configure)
    {
        configure(new StorageBuilder(services, name));
        return services;
    }

    /// <summary>
    ///     Register an action used to configure the same named <see cref="StorageOptions"/> options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureStorageOptions(this IServiceCollection services, Action<StorageOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="StorageOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureStorageOptions(this IServiceCollection services, string name, Action<StorageOptions> configureOptions) => services
        .Configure(name, configureOptions);

}
