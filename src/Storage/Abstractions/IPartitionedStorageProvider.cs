using System;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///    An abstraction over internal key based value-centric binary partitioned storage provider.
    /// </summary>
    /// <typeparam name="TValue">A value object type which specific partitioned storage implementation is assigned to.</typeparam>
    public interface IPartitionedStorageProvider<TValue> : IPartitionedStorage<StoreKey, byte[]>, IDisposable { }
}