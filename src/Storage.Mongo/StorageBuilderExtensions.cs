using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring MongoDB provider.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures storage to use MongoDB storage provider implementation.
    /// </summary>
    public static StorageBuilder UseMongo(this StorageBuilder builder) => builder
        .UseMongo(delegate { });

    /// <summary>
    ///     Configures storage to use MongoDB storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, string connectionString) => builder
        .UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures storage to use MongoDB storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .ConfigureMongoOptions(builder.Name, configureOptions)
            .AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures storage to use MongoDB storage provider implementation.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static StorageBuilder UseMongo(this StorageBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .ConfigureMongoOptions(builder.Name, configuration)
            .AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures MongoDB provider dependencies for storages.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddMongoProvider(this IServiceCollection services, string name) => services
        .TryAddScoped(typeof(MongoStorageProvider<>), typeof(MongoStorageProvider<>))
        .TryAddScoped(typeof(MongoHistoricalStorageProvider<>), typeof(MongoHistoricalStorageProvider<>))
        .TryAddScoped(typeof(MongoPartitionedStorageProvider<>), typeof(MongoPartitionedStorageProvider<>))
        .TryAddSingleton<IPostConfigureOptions<MongoOptions>, MongoPostConfigureOptions>()
        .ConfigureStorageOptions(name, o => o.UseMongo())
        .ConfigureMongoOptions(name, o => o.DatabaseName ??= MongoNames.DatabaseName)
        .AddMongoClient();
}
