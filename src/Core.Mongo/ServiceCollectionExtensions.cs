using Assistant.Net.Abstractions;
using Assistant.Net.Internal;
using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net;

/// <summary>
///     Service collection extensions for MongoDB providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the MongoDB <paramref name="connectionString"/> to configure <see cref="MongoOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, string connectionString) => services
        .Configure<MongoOptions>(name, o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Registers a configuration instance which <see cref="MongoOptions"/> will bind against.
    /// </summary>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<MongoOptions>(name, configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="MongoOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, Action<MongoOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Registers MongoDB client factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="ConfigureMongoOptions(IServiceCollection, string, string)"/> to configure.
    /// </remarks>
    public static IServiceCollection AddMongoClientFactory(this IServiceCollection services) => services
        .TryAddScoped<IMongoClientFactory, DefaultMongoClientFactory>();
}
