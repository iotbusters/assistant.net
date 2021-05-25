using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;

namespace Assistant.Net.Storage.Configuration
{
    public static class StorageBuilderExtensions
    {
        /// <summary>
        ///     Adds local storage for <typeparamref name="TValue"/> type.
        /// </summary>
        public static StorageBuilder AddLocal<TValue>(this StorageBuilder builder)
        {
            builder.Services.ReplaceScoped<IStorage<TValue>, LocalStorage<TValue>>();
            return builder;
        }

        /// <summary>
        ///     Adds local storage for any type.
        /// </summary>
        public static StorageBuilder AddLocalAny(this StorageBuilder builder)
        {
            builder.Services.ReplaceScoped(typeof(IStorage<>), typeof(LocalStorage<>));
            return builder;
        }
    }
}