using System;
using System.Threading.Tasks;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     An abstraction of storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
    /// <typeparam name="TValue">A value object type is stored.</typeparam>
    public interface IStorage<TKey, TValue>
    {
        /// <summary>
        ///    Tries to add a value associated to the <paramref name="key"/> if it doesn't exist.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="addFactory">
        ///    A factory method that resolves a value.
        ///    Pay attention, it will be called only if key doesn't exists.
        /// </param>
        /// <returns>An added or existed value.<returns>
        Task<TValue> AddOrGet(TKey key, Func<TKey, Task<TValue>> addFactory);

        /// <summary>
        ///    Tries to add or update an existing value associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="addFactory">
        ///    A factory method that resolves a value to be added.
        ///    Pay attention, it will be called only if key doesn't exists.
        /// </param>
        /// <param name="updateFactory">
        ///    A factory method that resolves a value to be updated.
        ///    Pay attention, it can be called multiple times.
        /// </param>
        /// <returns>An added or updated value.<returns>
        Task<TValue> AddOrUpdate(
            TKey key,
            Func<TKey, Task<TValue>> addFactory,
            Func<TKey, TValue, Task<TValue>> updateFactory);

        /// <summary>
        ///     Tries to find a <paramref name="value"/> associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="value">A found value.</param>
        /// <returns>An existed value if it was found.</returns>
        Task<Option<TValue>> TryGet(TKey key);

        /// <summary>
        ///     Tries to remove <paramref name="value"/> associated to the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key object.</param>
        /// <param name="value">A removed value.</param>
        /// <returns>A removed value if it was found.</returns>
        Task<Option<TValue>> TryRemove(TKey key);
    }

    /// <summary>
    ///    String key based value-centric storage.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
    public interface IStorage<TValue> : IStorage<string, TValue>, IDisposable { }
}