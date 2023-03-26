using Assistant.Net.Options;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage options extensions for SQLite provider.
/// </summary>
public static class StorageOptionsExtensions
{
    private static readonly Type sqliteProviderType = typeof(SqliteStorageProvider<>);
    private static readonly Type sqliteHistoricalProviderType = typeof(SqliteHistoricalStorageProvider<>);
    private static readonly Type sqlitePartitionedProviderType = typeof(SqlitePartitionedStorageProvider<>);

    /// <summary>
    ///     Configures storage to use a SQLite single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single provider.
    /// </remarks>
    public static StorageOptions UseSqliteSingleProvider(this StorageOptions options) => options
        .UseSingleProvider((p, valueType) =>
        {
            var implementationType = sqliteProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        }).UseSingleHistoricalProvider((p, valueType) =>
        {
            var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        }).UseSinglePartitionedProvider((p, valueType) =>
        {
            var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers SQLite storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqlite(this StorageOptions options, Type valueType)
    {
        options.Providers[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = sqliteProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers SQLite storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqliteAny(this StorageOptions options) => options
        .AddAny((p, valueType) =>
        {
            var implementationType = sqliteProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers SQLite historical storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqliteHistorical(this StorageOptions options, Type valueType)
    {
        options.HistoricalProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers SQLite historical storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqliteHistoricalAny(this StorageOptions options) => options
        .AddHistoricalAny((p, valueType) =>
        {
            var implementationType = sqliteHistoricalProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });

    /// <summary>
    ///     Registers SQLite partitioned storage provider of <paramref name="valueType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqlitePartitioned(this StorageOptions options, Type valueType)
    {
        options.PartitionedProviders[valueType] = new InstanceCachingFactory<object>(p =>
        {
            var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
        return options;
    }

    /// <summary>
    ///     Registers SQLite partitioned storage provider of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageOptions AddSqlitePartitionedAny(this StorageOptions options) => options
        .AddPartitionedAny((p, valueType) =>
        {
            var implementationType = sqlitePartitionedProviderType.MakeGenericType(valueType);
            return p.GetRequiredService(implementationType);
        });
}
