using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters
{
    internal class DefaultKeyConverter<TKey> : IKeyConverter<TKey>
    {
        private readonly IValueConverter<TKey> converter;

        public DefaultKeyConverter(ITypeEncoder typeEncoder, IValueConverter<TKey> converter)
        {
            KeyType = typeEncoder.Encode(typeof(TKey));
            this.converter = converter;
        }

        public async Task<StoreKey> Convert(TKey key)
        {
            var value = await converter.Convert(key);
            return new StoreKey(key.GetSha1(), KeyType, value);
        }

        public Task<TKey> Convert(StoreKey key)
        {
            if (key.Type != KeyType)
                throw new ArgumentException($"Expected key type '{KeyType}' instead of '{key.Type}'.");
            return converter.Convert(key.KeyValue);
        }

        public string KeyType { get; }
    }
}