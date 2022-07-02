using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System.Linq;
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
    ///     Gets all keys in the storage.
    /// </summary>
    IQueryable<KeyRecord> GetKeys();

    /// <summary>
    ///     Tries to remove a value associated to the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">A key object.</param>
    /// <param name="token"/>
    /// <returns>A removed value if it was found.</returns>
    Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token = default);
}
