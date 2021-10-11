using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Service collection extensions for MongoDB storage.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers a <see cref="MongoOptions"/> configuration from MongoDB <paramref name="connectionString"/>.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, string connectionString) => services
            .Configure<MongoOptions>(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Registers a configuration instance which <see cref="MongoOptions"/> will bind against.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<MongoOptions>(configuration);

        /// <summary>
        ///    Register an action used to configure <see cref="MongoOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, Action<MongoOptions> configureOptions) => services
            .Configure(configureOptions);

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

        /// <summary>
        ///     Registers <see cref="IMongoClient"/> implementation.
        /// </summary>
        /// <remarks>
        ///     Pay attention, you need to call explicitly one of overloaded <see cref="ConfigureMongoOptions(IServiceCollection, string)"/> to configure.
        /// </remarks>
        public static IServiceCollection AddMongoClient(this IServiceCollection services) => services
            .TryAddScoped<IMongoClient>(p =>
            {
                var connectionString = p.GetService<IOptions<MongoOptions>>()?.Value.ConnectionString
                                       ?? throw new InvalidOperationException($"{nameof(MongoOptions.ConnectionString)} is required. "
                                                                              + $"{nameof(MongoOptions)} weren't properly configured.");
                return new MongoClient(connectionString);
            });
    }
}
