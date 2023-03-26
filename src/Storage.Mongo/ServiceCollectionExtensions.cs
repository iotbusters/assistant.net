using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;

namespace Assistant.Net.Storage;

/// <summary>
///     Service collection extensions for MongoDB storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the MongoDB <paramref name="connectionString"/> to configure default <see cref="MongoOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string connectionString) => services
        .ConfigureMongoOptions(Microsoft.Extensions.Options.Options.DefaultName, o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Registers a configuration instance which default <see cref="MongoOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureMongoOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure default <see cref="MongoOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, Action<MongoOptions> configureOptions) => services
        .ConfigureMongoOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Registers the MongoDB <paramref name="connectionString"/> to configure <see cref="MongoOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, string connectionString) => services
        .ConfigureMongoOptions(name, o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Registers a configuration instance which the same named <see cref="MongoOptions"/> will bind against.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configuration">The application configuration values.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<MongoOptions>(name, configuration);

    /// <summary>
    ///    Register an action used to configure the same named <see cref="MongoOptions"/>.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string name, Action<MongoOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///     Registers named MongoDB client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="ConfigureMongoOptions(IServiceCollection, string, string)"/> to configure.
    /// </remarks>
    public static IServiceCollection AddMongoClient(this IServiceCollection services) => services
        .AddNamedOptionsContext()
        .TryAddScoped<IMongoClient>(p =>
        {
            var options = p.GetRequiredService<INamedOptions<MongoOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        })
        .TryAddScoped(p =>
        {
            var client = p.GetRequiredService<IMongoClient>();
            var options = p.GetRequiredService<INamedOptions<MongoOptions>>().Value;
            return client.GetDatabase(options.DatabaseName);
        });

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
