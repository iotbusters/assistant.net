using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal sealed class LocalStorageProvider<TValue> : IStorageProvider<TValue>
{
    private readonly ConcurrentDictionary<KeyRecord, ValueRecord> backedStorage = new();

    public Task<ValueRecord> AddOrGet(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token) =>
        Task.FromResult(
            backedStorage.GetOrAdd(
                key,
                valueFactory: _ => addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult()));

    public Task<ValueRecord> AddOrUpdate(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken _) =>
        Task.FromResult(
            backedStorage.AddOrUpdate(
                key,
                addValueFactory: k => addFactory(k).ConfigureAwait(false).GetAwaiter().GetResult(),
                updateValueFactory: (k, old) => updateFactory(k, old).ConfigureAwait(false).GetAwaiter().GetResult()));

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken _) =>
        Task.FromResult(
            backedStorage.TryGetValue(key, out var value) ? Option.Some(value) : Option.None);

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken _) =>
        Task.FromResult(
            backedStorage.TryRemove(key, out var value) ? Option.Some(value) : Option.None);

    public IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, CancellationToken token) =>
        backedStorage.Keys.AsQueryable().Where(predicate).AsAsync();
}
