using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage options extensions for MongoDB provider.
/// </summary>
public static class StorageOptionsExtensions
{
    /// <summary>
    ///     Configures storage to use a MongoDB storage provider implementation factories.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered storage provider factories.
    /// </remarks>
    public static StorageOptions UseMongo(this StorageOptions options) => options
        .UseStorage((p, valueType) =>
        {
            var implementationType = typeof(MongoStorageProvider<>).MakeGenericType(valueType);
            return (IStorageProvider)p.GetRequiredService(implementationType);
        })
        .UseHistoricalStorage((p, valueType) =>
        {
            var implementationType = typeof(MongoHistoricalStorageProvider<>).MakeGenericType(valueType);
            return (IHistoricalStorageProvider)p.GetRequiredService(implementationType);
        })
        .UsePartitionedStorage((p, valueType) =>
        {
            var implementationType = typeof(MongoPartitionedStorageProvider<>).MakeGenericType(valueType);
            return (IPartitionedStorageProvider)p.GetRequiredService(implementationType);
        });
}
