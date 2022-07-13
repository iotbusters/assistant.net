using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///    A specific data provider abstraction over internal key based value-centric binary storage.
/// </summary>
/// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
public interface IHistoricalStorageProvider<TValue> : IStorageProvider<TValue>
{
    /// <summary>
    ///     Tries to find a value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="version">A specific value version.</param>
    /// <param name="token"/>
    /// <returns>An existed value version if it was found.</returns>
    Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove historical value versions associated to the <paramref name="key"/> up to <paramref name="upToVersion"/> (including).
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="upToVersion">A specific value version.</param>
    /// <param name="token"/>
    /// <returns>A number of removed value versions.</returns>
    Task<Option<ValueRecord>> TryRemove(KeyRecord key, long upToVersion, CancellationToken token = default);
}
