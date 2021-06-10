using System;
using System.Text;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Converters
{
    internal class StringKeyConverter : IKeyConverter<string>
    {
        public string KeyType => nameof(String);

        public StoreKey Convert(string key) => new(key, KeyType, Encoding.UTF8.GetBytes(key));

        public string Convert(StoreKey key)
        {
            if (key.Type != KeyType)
                throw new ArgumentException($"Expected key type '{KeyType}' instead of '{key.Type}'.");
            return Encoding.UTF8.GetString(key.KeyValue);
        }
    }
}