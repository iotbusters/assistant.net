using System;
using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;

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

        public StoreKey Convert(TKey key) =>
            new(key.GetSha1(), KeyType, converter.Convert(key));

        public TKey Convert(StoreKey key)
        {
            if (key.Type != KeyType)
                throw new ArgumentException($"Expected key type '{KeyType}' instead of '{key.Type}'.");
            return converter.Convert(key.KeyValue);
        }

        public string KeyType { get; private set; }
    }
}