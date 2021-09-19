﻿using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Storage
git checkout {
    /// <summary>
    ///     Storage builder extensions for configuring MongoDB storages.
    /// </summary>
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder UseMongo(this StorageBuilder builder, Action<MongoOptions> configureOptions)
        {
            builder.Services
                .ConfigureMongoOptions(configureOptions)
                .AddMongoClient();
            return builder;
        }

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static StorageBuilder UseMongo(this StorageBuilder builder, IConfigurationSection config)
        {
            builder.Services
                .ConfigureMongoOptions(config)
                .AddMongoClient();
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
                .ReplaceSingleton(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds MongoDB storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddMongoAny(this StorageBuilder builder)
        {
            builder.Services
                .AddMongoClient()
                .ReplaceSingleton(typeof(IStorageProvider<>), typeof(MongoStorageProvider<>))
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
            var implementationType = typeof(MongoPartitionedStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .AddMongoClient()
                .ReplaceSingleton(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds partitioned MongoDB storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddMongoPartitionedAny(this StorageBuilder builder)
        {
            builder.Services
                .AddMongoClient()
                .ReplaceSingleton(typeof(IPartitionedStorageProvider<>), typeof(MongoPartitionedStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }
    }
}
