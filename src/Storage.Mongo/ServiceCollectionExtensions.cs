using Assistant.Net.Storage.Mongo.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Assistant.Net.Storage.Mongo
{
    /// <summary>
    ///     Service collection extensions for MongoDB storage.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Default MongoDB connection string name in configuration.
        /// </summary>
        public const string DefaultConnectionStringName = "StorageDatabase";

        /// <summary>
        ///     Registers a default <see cref="MongoOptions"/> configuration from the configured default connection string at:
        ///     <b>ConnectionStrings:StorageDatabase</b>.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services) => services
            .Configure<MongoOptions, IConfiguration>((o, c) => o.ConnectionString = c.GetConnectionString(DefaultConnectionStringName));

        /// <summary>
        ///     Registers a configuration instance which <see cref="MongoOptions"/> will bind against.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, IConfigurationSection config) => services
            .Configure<MongoOptions>(config);

        /// <summary>
        ///    Register an action used to configure <see cref="MongoOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, Action<MongoOptions> configureOptions) => services
            .Configure(configureOptions);

        /// <summary>
        ///     Registers <see cref="IMongoClient"/> implementation.
        /// </summary>
        /// <remarks>
        ///     Pay attention, you need to call explicitly one of overloaded <see cref="ConfigureMongoOptions(IServiceCollection)"/> to configure.
        /// </remarks>
        public static IServiceCollection AddMongoClient(this IServiceCollection services) => services
            .TryAddScoped<IMongoClient>(p =>
            {
                var connectionString = p.GetService<IOptions<MongoOptions>>()?.Value.ConnectionString ??
                                       throw new InvalidOperationException($"{nameof(MongoOptions)} weren't properly configured.");
                return new MongoClient(connectionString);
            });
    }
}
