using System;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     Storage extensions.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        ///     Tries to find a value by associated to the <paramref name="key"/> or return a default value.
        /// </summary>
        public static Task<TValue?> GetOrDefault<TKey, TValue>(
            this IStorage<TKey, TValue> storage,
            TKey key) =>
            storage.TryGet(key).GetValueOrDefault();

        /// <summary>
        ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
        /// </summary>
        public static Task<TValue> AddOrGet<TKey, TValue>(
            this IStorage<TKey, TValue> storage,
            TKey key,
            Func<TKey, TValue> addFactory) =>
            storage.AddOrGet(key, x => Task.FromResult(addFactory(x)));

        /// <summary>
        ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
        /// </summary>
        public static Task<TValue> AddOrGet<TKey, TValue>(
            this IStorage<TKey, TValue> storage,
            TKey key,
            TValue value) =>
            storage.AddOrGet(key, _ => Task.FromResult(value));

        /// <summary>
        ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
        /// </summary>
        public static Task<TValue> AddOrUpdate<TKey, TValue>(
            this IStorage<TKey, TValue> storage,
            TKey key,
            Func<TKey, TValue> addFactory,
            Func<TKey, TValue, TValue> updateFactory) =>
            storage.AddOrUpdate(
                key,
                key => Task.FromResult(addFactory(key)),
                (key, old) => Task.FromResult(updateFactory(key, old)));

        /// <summary>
        ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
        /// </summary>
        public static Task<TValue> AddOrUpdate<TKey, TValue>(
            this IStorage<TKey, TValue> storage,
            TKey key,
            TValue value) =>
            storage.AddOrUpdate(
                key,
                _ => Task.FromResult(value),
                (_, _) => Task.FromResult(value));
    }
}