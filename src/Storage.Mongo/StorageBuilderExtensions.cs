﻿using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Storage builder extensions for configuring MongoDB storages.
    /// </summary>
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Configures the storage to connect a MongoDB database by <paramref name="connectionString"/>.
        /// </summary>
        public static StorageBuilder UseMongo(this StorageBuilder builder, string connectionString) =>
            builder.UseMongo(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder UseMongo(this StorageBuilder builder, Action<MongoOptions> configureOptions)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(configureOptions)
                .ConfigureMongoStoringOptions(_ => { });
            return builder;
        }

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder UseMongo(this StorageBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(configuration)
                .ConfigureMongoStoringOptions(_ => { });
            return builder;
        }

        /// <summary>
        ///     Configures storing mechanism to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder ConfigureMongoStoring(this StorageBuilder builder, Action<MongoStoringOptions> configureOptions)
        {
            builder.Services.ConfigureMongoStoringOptions(configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures storing mechanism to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder ConfigureMongoStoring(this StorageBuilder builder, IConfigurationSection configuration)
        {
            builder.Services.ConfigureMongoStoringOptions(configuration);
            return builder;
        }

        /// <summary>
        ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddMongo<TKey, TValue>(this StorageBuilder builder) => builder
            .AddMongo(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddMongo(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(MongoStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds MongoDB storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddMongoAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IStorageProvider<>), typeof(MongoStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddMongoHistorical<TKey, TValue>(this StorageBuilder builder) => builder
            .AddMongoHistorical(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddMongoHistorical(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IHistoricalStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(MongoHistoricalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds MongoDB storage for any unregistered type
        ///     including value change history.
        /// </summary>
        public static StorageBuilder AddMongoHistoricalAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IStorageProvider<>), typeof(MongoHistoricalStorageProvider<>))
                .ReplaceScoped(typeof(IHistoricalStorageProvider<>), typeof(MongoHistoricalStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds partitioned MongoDB storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddMongoPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
            .AddMongoPartitioned(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds partitioned MongoDB storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddMongoPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IPartitionedStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(GenericPartitionedStorageProvider<>).MakeGenericType(valueType);
            var backedServiceType = typeof(IHistoricalStorageProvider<>).MakeGenericType(valueType);
            var backedImplementationType = typeof(MongoHistoricalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .TryAddScoped(backedServiceType, backedImplementationType)
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds partitioned MongoDB storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddMongoPartitionedAny(this StorageBuilder builder)
        {
            builder.Services
                .TryAddScoped(typeof(IHistoricalStorageProvider<>), typeof(MongoHistoricalStorageProvider<>))
                .ReplaceScoped(typeof(IPartitionedStorageProvider<>), typeof(GenericPartitionedStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }
    }
}
