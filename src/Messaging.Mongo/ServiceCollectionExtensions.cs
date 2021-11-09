using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Service collection extensions for MongoDb based remote message handling.
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
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, IConfigurationSection config) => services
            .Configure<MongoOptions>(config);

        /// <summary>
        ///    Register an action used to configure <see cref="MongoOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoOptions(this IServiceCollection services, Action<MongoOptions> configureOptions) => services
            .Configure(configureOptions);

        /// <summary>
        ///     Registers MongoDB client factory for messaging provider.
        /// </summary>
        /// <remarks>
        ///     Pay attention, you need to call explicitly one of overloaded <see cref="ConfigureMongoOptions(IServiceCollection, string)"/> to configure.
        /// </remarks>
        public static IServiceCollection AddMongoClientFactory(this IServiceCollection services) => services
            .TryAddScoped<IMongoClientFactory, DefaultMongoClientFactory>();
    }
}
