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
        ///     Pay attention, all storing types should be previously registered.
        /// </summary>
        public static IServiceCollection AddStorage(this IServiceCollection services, Action<StorageBuilder> configureOptions)
        {
            configureOptions(new StorageBuilder(services));
            return services
                // todo: is still needed?
                .AddSerializer(b => b.AddJsonType<KeyRecord>())
                .AddTypeEncoder()
                // todo: resolve single instance per storage type
                .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
                .TryAddScoped(typeof(IAdminStorage<,>), typeof(Storage<,>))
                .TryAddScoped(typeof(IPartitionedStorage<,>), typeof(PartitionedStorage<,>))
                .TryAddSingleton<IKeyConverter<string>, StringKeyConverter>()
                .TryAddSingleton(typeof(IKeyConverter<>), typeof(DefaultKeyConverter<>))
                .TryAddSingleton(typeof(IValueConverter<>), typeof(DefaultValueConverter<>));
        }
    }
}