using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     An extended abstraction over storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
///     that exposes all stored keys.
/// </summary>
/// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
/// <typeparam name="TValue">A value object type is stored.</typeparam>
public interface IAdminStorage<TKey, TValue> : IStorage<TKey, TValue>
{
    /// <summary>
    ///     Gets all keys in the storage.
    /// </summary>
    IAsyncEnumerable<TKey> GetKeys(CancellationToken token = default);

    /// <summary>
    ///     Tries to find a value audit associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="token"/>
    /// <returns>An existed value audit if it was found.</returns>
    Task<Option<Audit>> TryGetAudit(TKey key, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove a value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="token"/>
    /// <returns>A removed value if it was found.</returns>
    Task<Option<TValue>> TryRemove(TKey key, CancellationToken token = default);
}
