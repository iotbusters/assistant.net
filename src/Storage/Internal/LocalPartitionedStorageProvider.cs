using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal sealed class LocalPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
{
    private readonly LocalHistoricalStorageProvider<TValue> provider;

    public LocalPartitionedStorageProvider(LocalHistoricalStorageProvider<TValue> provider) =>
        this.provider = provider;

    public Task<ValueRecord> AddOrGet(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token) => provider
        .AddOrGet(key, addFactory, token);

    public Task<ValueRecord> AddOrUpdate(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken token) => provider
        .AddOrUpdate(key, addFactory, updateFactory, token);

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) => provider
        .TryGet(key, token);

    public IQueryable<KeyRecord> GetKeys() => provider
        .GetKeys();

    public async Task<long> Add(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken token)
    {
        var value = await AddOrUpdate(key, addFactory, updateFactory, token);
        return value.Audit.Version;
    }

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) => provider
        .TryRemove(key, token);

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token) => provider
        .TryGet(key, version, token);

    public Task<long> TryRemove(KeyRecord key, long upToVersion, CancellationToken token) => provider
        .TryRemove(key, upToVersion, token);

    void IDisposable.Dispose() => ((IDisposable)provider).Dispose();
}
