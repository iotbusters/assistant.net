using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal sealed class LocalPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
{
    private readonly LocalHistoricalStorageProvider<TValue> provider;

    public LocalPartitionedStorageProvider(LocalHistoricalStorageProvider<TValue> provider) =>
        this.provider = provider;

    public Task<ValueRecord> AddOrGet(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token) =>
        provider.AddOrGet(key, addFactory, token);

    public Task<ValueRecord> AddOrUpdate(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken token) =>
        provider.AddOrUpdate(key, addFactory, updateFactory, token);

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) =>
        provider.TryGet(key, token);

    public IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, CancellationToken token) =>
        provider.GetKeys(predicate, token);

    public async Task<ValueRecord> Add(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken token) =>
        await AddOrUpdate(key, addFactory, updateFactory, token);

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) =>
        provider.TryRemove(key, token);

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token) =>
        provider.TryGet(key, version, token);

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, long upToVersion, CancellationToken token) =>
        provider.TryRemove(key, upToVersion, token);
}
