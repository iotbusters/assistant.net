using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal
{
    internal class LocalPartitionedStorageProvider<TValue> : IPartitionedStorageProvider<TValue>
    {
        private readonly ConcurrentDictionary<KeyRecord, ConcurrentDictionary<long, ValueRecord>> backedStorage = new();

        public Task<long> Add(KeyRecord key, ValueRecord value, CancellationToken _)
        {
            var partition = backedStorage.GetOrAdd(key, _ => new ConcurrentDictionary<long, ValueRecord>());

            // todo: replace infinite loop
            long index = partition.Count;
            while (!partition.TryAdd(index, value))
                index = partition.Count;

            return Task.FromResult(index);
        }

        public Task<Option<ValueRecord>> TryGet(KeyRecord key, long index, CancellationToken _)
        {
            if (backedStorage.TryGetValue(key, out var partition)
                && partition.TryGetValue(index, out var value))
                return Task.FromResult(Option.Some(value));
            return Task.FromResult<Option<ValueRecord>>(Option.None);
        }

        public IQueryable<KeyRecord> GetKeys() => backedStorage.Keys.AsQueryable();

        public Task<long> TryRemove(KeyRecord key, long upToIndex, CancellationToken token = default)
        {
            if (!backedStorage.TryGetValue(key, out var partition))
                return Task.FromResult(0L);

            var counter = 0L;
            foreach (var index in partition.Keys.OrderBy(_ => _).TakeWhile(x => x <= upToIndex))
                if (partition.TryRemove(index, out _))
                    counter++;

            return Task.FromResult(counter);
        }

        public void Dispose() { }
    }
}
