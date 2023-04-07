using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage options extensions for SQLite provider.
/// </summary>
public static class StorageOptionsExtensions
{
    /// <summary>
    ///     Configures storage to use a SQLite storage provider implementation factories.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered storage provider factories.
    /// </remarks>
    public static StorageOptions UseSqlite(this StorageOptions options) => options
        .UseStorage((p, valueType) =>
        {
            var implementationType = typeof(SqliteStorageProvider<>).MakeGenericType(valueType);
            return (IStorageProvider)p.GetRequiredService(implementationType);
        })
        .UseHistoricalStorage((p, valueType) =>
        {
            var implementationType = typeof(SqliteHistoricalStorageProvider<>).MakeGenericType(valueType);
            return (IHistoricalStorageProvider)p.GetRequiredService(implementationType);
        })
        .UsePartitionedStorage((p, valueType) =>
        {
            var implementationType = typeof(SqlitePartitionedStorageProvider<>).MakeGenericType(valueType);
            return (IPartitionedStorageProvider)p.GetRequiredService(implementationType);
        });
}
