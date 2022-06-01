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
    ///     Registers a configuration instance which default <see cref="MongoStoringOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureMongoStoringOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure default <see cref="MongoStoringOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, Action<MongoStoringOptions> configureOptions) => services
        .ConfigureMongoStoringOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers a configuration instance which the same named <see cref="MongoStoringOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<MongoStoringOptions>(name, configuration);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="MongoStoringOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMongoStoringOptions(this IServiceCollection services, string name, Action<MongoStoringOptions> configureOptions) => services
        .Configure(name, configureOptions);
}
