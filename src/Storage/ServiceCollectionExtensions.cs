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
    ///     Adds common services required by storage implementations.
    /// </summary>
    public static IServiceCollection AddStorage(this IServiceCollection services) => services
        .AddLogging()
        .AddSystemClock()
        .AddDiagnostics()
        .AddTypeEncoder()
        .AddSerializer()
        .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
        .TryAddScoped(typeof(IAdminStorage<,>), typeof(Storage<,>))
        .TryAddScoped(typeof(IHistoricalStorage<,>), typeof(HistoricalStorage<,>))
        .TryAddScoped(typeof(IHistoricalAdminStorage<,>), typeof(HistoricalStorage<,>))
        .TryAddScoped(typeof(IPartitionedStorage<,>), typeof(PartitionedStorage<,>))
        .TryAddScoped(typeof(IPartitionedAdminStorage<,>), typeof(PartitionedStorage<,>))
        .TryAddSingleton(typeof(IValueConverter<>), typeof(TypedValueConverter<>))
        // todo: optimize converters. e.g. converter.CanConvert(type)
        .TryAddSingleton<PrimitiveValueConverter>()
        .TryAddSingleton<IValueConverter<string>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<Guid>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<bool>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<int>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<float>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<double>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<decimal>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<TimeSpan>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<DateTime>>(p => p.GetRequiredService<PrimitiveValueConverter>())
        .TryAddSingleton<IValueConverter<DateTimeOffset>>(p => p.GetRequiredService<PrimitiveValueConverter>());

    /// <summary>
    ///     Adds common services required by storage implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, all storing types should be previously registered.
    /// </remarks>
    public static IServiceCollection AddStorage(this IServiceCollection services, Action<StorageBuilder> configure) => services
        .AddStorage()
        .ConfigureStorage(configure);

    /// <summary>
    ///     Configures storage implementations, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configure">The action used to configure the default option instances.</param>
    public static IServiceCollection ConfigureStorage(this IServiceCollection services, Action<StorageBuilder> configure)
    {
        configure(new StorageBuilder(services, Microsoft.Extensions.Options.Options.DefaultName));
        return services;
    }

    /// <summary>
    ///     Configures storage implementations, required services and options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configure">The action used to configure the default option instances.</param>
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
