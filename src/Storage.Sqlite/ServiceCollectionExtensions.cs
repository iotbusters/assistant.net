using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Service collection extensions for MongoDB storage.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers a <see cref="SqliteOptions"/> configuration from MongoDB <paramref name="connectionString"/>.
        /// </summary>
        public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, string connectionString) => services
            .Configure<SqliteOptions>(o => o.Connection(connectionString));

        /// <summary>
        ///     Registers a configuration instance which <see cref="SqliteOptions"/> will bind against.
        /// </summary>
        public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<SqliteOptions>(configuration);

        /// <summary>
        ///    Register an action used to configure <see cref="SqliteOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureSqliteOptions(this IServiceCollection services, Action<SqliteOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}
