using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Service collection extensions for MongoDB storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a configuration instance which <see cref="MongoStoringOptions"/> will bind against.
    /// </summary>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<MongoStoringOptions>(configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="MongoStoringOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, Action<MongoStoringOptions> configureOptions) => services
        .Configure(configureOptions);
}