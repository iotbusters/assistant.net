using Assistant.Net.Unions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     An abstraction over storing mechanism for specific <typeparamref name="TKey"/> and <typeparamref name="TValue"/>
///     including value change history.
/// </summary>
/// <typeparam name="TKey">A key object type that uniquely associated with <typeparamref name="TValue"/>.</typeparam>
/// <typeparam name="TValue">A value object type is stored.</typeparam>
public interface IHistoricalStorage<TKey, TValue> : IStorage<TKey, TValue>
{
    /// <summary>
    ///     Tries to find a value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="version">A specific value version.</param>
    /// <param name="token"/>
    /// <returns>An existed value if it was found.</returns>
    Task<Option<TValue>> TryGet(TKey key, long version, CancellationToken token = default);
}
