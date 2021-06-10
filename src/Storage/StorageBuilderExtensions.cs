using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Configuration;
using Assistant.Net.Storage.Internal;

namespace Assistant.Net.Storage
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Adds local storage for <typeparamref name="TValue"/> type.
        /// </summary>
        public static StorageBuilder AddLocal<TValue>(this StorageBuilder builder)
        {
            builder.Services.ReplaceScoped<IStorageProvider<TValue>, LocalStorageProvider<TValue>>();
            return builder;
        }

        /// <summary>
        ///     Adds local storage for any type.
        /// </summary>
        public static StorageBuilder AddLocalAny(this StorageBuilder builder)
        {
            builder.Services.ReplaceScoped(typeof(IStorageProvider<>), typeof(LocalStorageProvider<>));
            return builder;
        }

        /// <summary>
        ///     Adds local partitioned storage for <typeparamref name="TValue"/> type.
        /// </summary>
        public static StorageBuilder AddLocalPartitioned<TValue>(this StorageBuilder builder)
        {
            builder.Services.ReplaceScoped<IPartitionedStorageProvider<TValue>, LocalPartitionedStorageProvider<TValue>>();
            return builder;
        }
    }
}