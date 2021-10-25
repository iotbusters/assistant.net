using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Assistant.Net.Storage
{
    /// <summary>
    ///     Storage builder extensions for configuring local storages.
    /// </summary>
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddLocal<TKey, TValue>(this StorageBuilder builder) => builder
            .AddLocal(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddLocal(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(LocalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceSingleton(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds local storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddLocalAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceSingleton(typeof(IStorageProvider<>), typeof(LocalStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds local storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddHistoricalLocal<TKey, TValue>(this StorageBuilder builder) => builder
            .AddHistoricalLocal(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds local storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddHistoricalLocal(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IHistoricalStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(LocalHistoricalStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceSingleton(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds local storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddHistoricalLocalAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceSingleton(typeof(IHistoricalStorageProvider<>), typeof(LocalHistoricalStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Adds local partitioned storage of <typeparamref name="TValue"/> value type with <typeparamref name="TKey"/> key type.
        /// </summary>
        public static StorageBuilder AddLocalPartitioned<TKey, TValue>(this StorageBuilder builder) => builder
            .AddLocalPartitioned(typeof(TKey), typeof(TValue));

        /// <summary>
        ///     Adds local partitioned storage of <paramref name="valueType"/> with <paramref name="keyType"/>.
        /// </summary>
        public static StorageBuilder AddLocalPartitioned(this StorageBuilder builder, Type keyType, Type valueType)
        {
            var serviceType = typeof(IPartitionedStorageProvider<>).MakeGenericType(valueType);
            var implementationType = typeof(LocalPartitionedStorageProvider<>).MakeGenericType(valueType);

            builder.Services
                .ReplaceSingleton(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds local partitioned storage for any unregistered type.
        /// </summary>
        public static StorageBuilder AddLocalPartitionedAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceSingleton(typeof(IPartitionedStorageProvider<>), typeof(LocalPartitionedStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }

        /// <summary>
        ///     Apply a configuration type <typeparamref name="TConfiguration" />.
        /// </summary>
        public static StorageBuilder AddConfiguration<TConfiguration>(this StorageBuilder builder)
            where TConfiguration : IStorageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

        /// <summary>
        ///     Apply a list of configuration instances <paramref name="storageConfigurations" />.
        /// </summary>
        public static StorageBuilder AddConfiguration(this StorageBuilder builder, params IStorageConfiguration[] storageConfigurations)
        {
            foreach (var config in storageConfigurations)
                config.Configure(builder);
            return builder;
        }

    }
}
