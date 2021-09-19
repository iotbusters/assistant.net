using Assistant.Net.Storage.Models;
using System;
using System.Linq;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    An abstraction over internal key based value-centric binary storage provider.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
    public interface IStorageProvider<TValue> : IStorage<KeyRecord, ValueRecord>, IDisposable
    {
        /// <summary>
        ///     Gets all keys in the storage.
        /// </summary>
        IQueryable<KeyRecord> GetKeys();
    }
}
