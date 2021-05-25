using System;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;

namespace Assistant.Net.Storage.Internal
{
    public abstract class BinaryStorage<TValue> : IStorage<TValue>
    {
        private readonly IBinaryStorage backedStorage;
        private readonly IValueBinaryConverter<TValue> valueConverter;

        public BinaryStorage(IBinaryStorage backedStorage, IValueBinaryConverter<TValue> valueConverter)
        {
            this.backedStorage = backedStorage;
            this.valueConverter = valueConverter;
        }

        public async Task<TValue> AddOrGet(string key, Func<string, Task<TValue>> addFactory)
        {
            var bytes = await backedStorage.AddOrGet(
                key,
                async key =>
                {
                    var value = await addFactory(key);
                    return valueConverter.Convert(value);
                });
            return valueConverter.Convert(bytes);
        }

        public async Task<TValue> AddOrUpdate(
            string key,
            Func<string, Task<TValue>> addFactory,
            Func<string, TValue, Task<TValue>> updateFactory)
        {
            var bytes = await backedStorage.AddOrUpdate(
                key,
                async key =>
                {
                    var value = await addFactory(key);
                    return valueConverter.Convert(value);
                },
                async (key, old) =>
                {
                    var oldValue = valueConverter.Convert(old);
                    var newValue = await updateFactory(key, oldValue);
                    return valueConverter.Convert(newValue);
                });
            return valueConverter.Convert(bytes);
        }

        public async Task<Option<TValue>> TryGet(string key)
        {
            var bytes = await backedStorage.TryGet(key);
            return bytes.Map(valueConverter.Convert);
        }

        public async Task<Option<TValue>> TryRemove(string key)
        {
            var bytes = await backedStorage.TryRemove(key);
            return bytes.Map(valueConverter.Convert);
        }

        void IDisposable.Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose() => backedStorage.Dispose();

        ~BinaryStorage() => ((IDisposable)this).Dispose();
    }
}