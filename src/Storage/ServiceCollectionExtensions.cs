using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Converters;
using Assistant.Net.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Service collection extensions for storages.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds common services required by storage implementation.
        /// </summary>
        /// <remarks>
        ///     Pay attention, all storing types should be previously registered.
        /// </remarks>
        public static IServiceCollection AddStorage(this IServiceCollection services, Action<StorageBuilder> configureOptions)
        {
            configureOptions(new StorageBuilder(services));
            return services
                .AddSystemClock()
                .AddSerializer()
                .AddTypeEncoder()
                // todo: resolve single instance per storage type
                .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
                .TryAddScoped(typeof(IAdminStorage<,>), typeof(Storage<,>))
                .TryAddScoped(typeof(IPartitionedStorage<,>), typeof(PartitionedStorage<,>))
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
        }
    }
}
