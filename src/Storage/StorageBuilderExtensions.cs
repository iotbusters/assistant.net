using System;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;

namespace Assistant.Net.Storage
{
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
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds local storage for any type.
        /// </summary>
        public static StorageBuilder AddLocalAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IStorageProvider<>), typeof(LocalStorageProvider<>))
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
                .ReplaceScoped(serviceType, implementationType)
                .ConfigureSerializer(b => b.AddJsonType(keyType).AddJsonType(valueType));
            return builder;
        }

        /// <summary>
        ///     Adds local partitioned storage for any type.
        /// </summary>
        public static StorageBuilder AddLocalPartitionedAny(this StorageBuilder builder)
        {
            builder.Services
                .ReplaceScoped(typeof(IPartitionedStorageProvider<>), typeof(LocalPartitionedStorageProvider<>))
                .ConfigureSerializer(b => b.AddJsonTypeAny());
            return builder;
        }
    }
}