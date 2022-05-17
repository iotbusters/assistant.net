using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///    A specific data provider abstraction over internal key based value-centric binary partitioned storage.
/// </summary>
/// <typeparam name="TValue">A value object type which specific partitioned storage implementation is assigned to.</typeparam>
public interface IPartitionedStorageProvider<TValue> : IStorage<KeyRecord, ValueRecord>, IDisposable
{
    /// <summary>
    ///     Gets all keys in the storage.
    /// </summary>
    IQueryable<KeyRecord> GetKeys();

    /// <summary>
    ///    Adds next indexed value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="addFactory">
    ///    A factory method that resolves a value to be added initially.
    ///    Pay attention, it will be called only if key doesn't exists.
    /// </param>
    /// <param name="updateFactory">
    ///    A factory method that resolves a value to be added.
    ///    Pay attention, it can be called multiple times.
    /// </param>
    /// <param name="token"/>
    /// <returns>An added index.</returns>
    Task<long> Add(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token = default);

    /// <summary>
    ///     Tries to find a value in partition associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="index">An index of specific object under the <paramref name="key"/>.</param>
    /// <param name="token"/>
    /// <returns>An existed value if it was found.</returns>
    Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove all values associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="token"/>
    /// <returns>A removed value if it was found.</returns>
    Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove values below the <paramref name="upToIndex"/> inclusive associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A partition key object.</param>
    /// <param name="upToIndex">An index of specific object under the <paramref name="key"/>.</param>
    /// <param name="token"/>
    /// <returns>A number of removed values.</returns>
    Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token = default);
}
