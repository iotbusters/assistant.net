using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///    An abstraction over internal key based value-centric binary storage provider.
/// </summary>
/// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
public interface IStorageProvider<TValue> : IStorage<KeyRecord, ValueRecord>
{
    /// <summary>
    ///     Iterates keys of the storage.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="token"/>
    /// <returns>An async iterator over keys of the storage.</returns>
    IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, CancellationToken token = default);

    /// <summary>
    ///     Tries to remove a value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="token"/>
    /// <returns>A removed value if it was found.</returns>
    Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token = default);
}
