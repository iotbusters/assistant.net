using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Converters;
using Assistant.Net.Storage.Configuration;

namespace Assistant.Net.Storage
{
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
                .AddSerializer(b => b.AddJsonType<StoreKey>())
                .AddTypeEncoder()
                .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
                .TryAddScoped(typeof(IPartitionedStorage<,>), typeof(PartitionedStorage<,>))
                .TryAddSingleton<IKeyConverter<string>, StringKeyConverter>()
                .TryAddSingleton(typeof(IKeyConverter<>), typeof(DefaultKeyConverter<>))
                .TryAddSingleton(typeof(IValueConverter<>), typeof(DefaultValueConverter<>));
        }
    }
}