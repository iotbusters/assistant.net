using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Storage builder extensions for configuring SQLite storages.
    /// </summary>
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Configures the storage to connect a SQLite database using <paramref name="connection"/>.
        /// </summary>
        public static StorageBuilder UseSqlite(this StorageBuilder builder, SqliteConnection connection)
        {
            builder.Services.ReplaceSingleton(_ =>
            {
                connection.Open();
                return connection;
            });
            // the connection string won't be used but only it just reflects provided connection.
            return builder.UseSqlite(o => o.Connection(connection.ConnectionString));
        }

        /// <summary>
        ///     Configures the storage to connect a SQLite database by <paramref name="connectionString"/>.
        /// </summary>
        public static StorageBuilder UseSqlite(this StorageBuilder builder, string connectionString) => builder
            .UseSqlite(o => o.Connection(connectionString));

        /// <summary>
        ///     Configures the storage to connect a SQLite database.
        /// </summary>
        public static StorageBuilder UseSqlite(this StorageBuilder builder, Action<SqliteOptions> configureOptions)
        {
            builder.Services
                .AddDbContext()
                .ConfigureSqliteOptions(configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures the storage to connect a SQLite database.
        /// </summary>
        public static StorageBuilder UseSqlite(this StorageBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddDbContext()
                .ConfigureSqliteOptions(configuration);
            return builder;
        }

        /// <summary>
        ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddSqlite<TKey, TValue>(this StorageBuilder builder) => builder
            .AddSqlite(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddSqlite(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(SqliteStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds SQLite storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddSqliteAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IStorageProvider<>), typeof(SqliteStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddSqliteHistorical<TKey, TValue>(this StorageBuilder builder) => builder
            .AddSqliteHistorical(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddSqliteHistorical(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IHistoricalStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(SqliteHistoricalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds SQLite storage for any unregistered type
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddSqliteHistoricalAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IStorageProvider<>), typeof(SqliteHistoricalStorageProvider<>))
                .ReplaceScoped(typeof(IHistoricalStorageProvider<>), typeof(SqliteHistoricalStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds partitioned SQLite storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddSqlitePartitioned<TKey, TValue>(this StorageBuilder builder) => builder
            .AddSqlitePartitioned(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds partitioned SQLite storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddSqlitePartitioned(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IPartitionedStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(GenericPartitionedStorageProvider<>).MakeGenericType(valueType);
            var backedServiceType = typeof(IHistoricalStorageProvider<>).MakeGenericType(valueType);
            var backedImplementationType = typeof(SqliteHistoricalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .TryAddScoped(backedServiceType, backedImplementationType)
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds partitioned SQLite storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddSqlitePartitionedAny(this StorageBuilder builder)
        {
            builder.Services
                .TryAddScoped(typeof(IHistoricalStorageProvider<>), typeof(SqliteHistoricalStorageProvider<>))
                .ReplaceScoped(typeof(IPartitionedStorageProvider<>), typeof(GenericPartitionedStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services) => services
            .TryAddSingleton(p =>
            {
                var connectionString = p.GetRequiredService<IOptions<SqliteOptions>>().Value.ConnectionString;
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection;
            })
            .AddPooledDbContextFactory<StorageDbContext>((p, b) =>
            {
                var connection = p.GetRequiredService<SqliteConnection>();
                b.UseSqlite(connection);
            })
            .AddPooledDbContextFactory<HistoricalStorageDbContext>((p, b) =>
            {
                var connection = p.GetRequiredService<SqliteConnection>();
                b.UseSqlite(connection);
            });
    }
}
