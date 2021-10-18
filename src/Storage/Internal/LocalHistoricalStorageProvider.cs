using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
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

        public Task<ValueRecord> AddOrUpdate(
            KeyRecord key,
            Func<KeyRecord, Task<ValueRecord>> addFactory,
            Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
            CancellationToken _)
        {
            var versions = backedStorage.GetOrAdd(key, valueFactory: _ => new());

            while (true)
                if (!versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out var currentVersion))
                {
                    var added = addFactory(key).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (versions.TryAdd(1, added))
                        return Task.FromResult(added with {Audit = new Audit(added.Audit.Details, version: 1)});
                }
                else
                {
                    var newVersion = versions.Keys.Max() + 1;
                    var added = updateFactory(key, currentVersion).ConfigureAwait(false).GetAwaiter().GetResult();
                    if(versions.TryAdd(newVersion, added))
                        return Task.FromResult(added with { Audit = new Audit(added.Audit.Details, newVersion) });
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
            if(!backedStorage.TryRemove(key, out var versions))
                return Task.FromResult<Option<ValueRecord>>(Option.None);

            return Task.FromResult(
                versions.TryGetValue(versions.Keys.DefaultIfEmpty(0).Max(), out var value)
                    ? Option.Some(value)
                    : Option.None);
        }

        public Task<long> TryRemove(KeyRecord key, long upToVersion, CancellationToken _)
        {
            if (!backedStorage.TryGetValue(key, out var versions))
                return Task.FromResult(0L);

            var count = 0L;
            foreach (var version in versions.Keys.OrderBy(x => x).Where(x => x <= upToVersion))
                if (versions.TryRemove(version, out var _))
                    count++;

            if (!versions.Any())
                if (backedStorage.TryRemove(key, out var latestVersions))
                    count += latestVersions.Count;// note: race condition.

            return Task.FromResult(count);
        }

        public IQueryable<KeyRecord> GetKeys() => backedStorage.Keys.AsQueryable();

        void IDisposable.Dispose() { }
    }
}
