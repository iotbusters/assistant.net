using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     An extended abstraction over storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
///     including value change history that exposes all stored keys.
/// </summary>
/// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
/// <typeparam name="TValue">A value object type is stored.</typeparam>
public interface IHistoricalAdminStorage<TKey, TValue> : IHistoricalStorage<TKey, TValue>, IAdminStorage<TKey, TValue>
{
    /// <summary>
    ///     Tries to remove historical value versions associated to the <paramref name="key"/>
    ///     up to <paramref name="upToVersion"/> (including).
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="upToVersion">A specific value version.</param>
    /// <param name="token"/>
    /// <returns>A number of removed value versions.</returns>
    Task<long> TryRemove(TKey key, long upToVersion, CancellationToken token = default);
}
