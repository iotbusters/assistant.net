using System;
using System.Text;
using System.Threading.Tasks;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Converters
{
    internal class StringKeyConverter : IKeyConverter<string>
    {
        public string KeyType => nameof(String);

        public Task<StoreKey> Convert(string key) =>
            Task.FromResult(new StoreKey(key, KeyType, Encoding.UTF8.GetBytes(key)));

        public Task<string> Convert(StoreKey key)
        {
            if (key.Type != KeyType)
                throw new ArgumentException($"Expected key type '{KeyType}' instead of '{key.Type}'.");
            return Task.FromResult(Encoding.UTF8.GetString(key.KeyValue));
        }
    }
}