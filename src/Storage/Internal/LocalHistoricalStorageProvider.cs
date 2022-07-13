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

internal sealed class LocalHistoricalStorageProvider<TValue> : IHistoricalStorageProvider<TValue>
{
    private readonly ConcurrentDictionary<KeyRecord, ConcurrentDictionary<long, ValueRecord>> backedStorage = new();

    public Task<ValueRecord> AddOrGet(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, CancellationToken token)
    {
        var versions = backedStorage.GetOrAdd(key, valueFactory: _ => new());

        ValueRecord? currentVersion;
        while (!versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out currentVersion))
        {
            var added = addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult();
            versions.TryAdd(1, added with { Audit = new Audit(added.Audit.Details, version: 1) });
        }

        return Task.FromResult(currentVersion);
    }

    public Task<ValueRecord> AddOrUpdate(KeyRecord key, Func<KeyRecord, Task<ValueRecord>> addFactory, Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory, CancellationToken _)
    {
        var versions = backedStorage.GetOrAdd(key, valueFactory: _ => new());

        while (true)
            if (!versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out var currentVersion))
            {
                var added = addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult();
                if (versions.TryAdd(added.Audit.Version, added))
                    return Task.FromResult(added);
            }
            else
            {
                var added = updateFactory(key, currentVersion).ConfigureAwait(false).GetAwaiter().GetResult();
                if(versions.TryAdd(added.Audit.Version, added))
                    return Task.FromResult(added);
            }
    }

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken _) =>
        Task.FromResult(
            backedStorage.TryGetValue(key, out var versions)
            && versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out var value)
                ? Option.Some(value)
                : Option.None);

    public Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken _) =>
        Task.FromResult(
            backedStorage.TryGetValue(key, out var versions) && versions.TryGetValue(version, out var value)
                ? Option.Some(value)
                : Option.None);

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken _)
    {
        if (!backedStorage.TryRemove(key, out var versions))
            return Task.FromResult<Option<ValueRecord>>(Option.None);

        return Task.FromResult(
            versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out var value)
                ? Option.Some(value)
                : Option.None);
    }

    public Task<Option<ValueRecord>> TryRemove(KeyRecord key, long upToVersion, CancellationToken _)
    {
        if (!backedStorage.TryGetValue(key, out var versions))
            return Task.FromResult<Option<ValueRecord>>(Option.None);

        var value = versions[Math.Min(upToVersion, versions.Keys.Max())];

        foreach (var version in versions.Keys.Where(x => x <= upToVersion))
            versions.TryRemove(version, out var _);

        if (!versions.Any())
            backedStorage.TryRemove(key, out var _);

        return Task.FromResult(Option.Some(value));
    }

    public IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, CancellationToken token) =>
        backedStorage.Keys.AsQueryable().Where(predicate).AsAsync();
}
