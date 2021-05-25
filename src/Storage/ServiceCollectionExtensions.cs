using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Configuration;

namespace Assistant.Net.Storage
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds common services required by storage implementation.
        ///     Pay attention, all storing types should be registered.
        /// </summary>
        public static IServiceCollection AddStorage(this IServiceCollection services, Action<StorageBuilder> configureOptions)
        {
            configureOptions(new StorageBuilder(services));
            return services
                .TryAddScoped(typeof(IStorage<,>), typeof(Storage<,>))
                .TryAddSingleton<IKeyConverter<string>, StringKeyConverter>()
                .TryAddSingleton(typeof(IKeyConverter<>), typeof(KeyConverter<>));
        }
    }
}