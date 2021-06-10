using System;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    Internal key based value-centric binary storage provider.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific storage implementation is assigned to.</typeparam>
    public interface IStorageProvider<TValue> : IStorage<StoreKey, byte[]>, IDisposable { }
}