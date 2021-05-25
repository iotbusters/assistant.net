using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Utils;

namespace Assistant.Net.Storage.Internal
{
    internal class KeyConverter<TKey> : IKeyConverter<TKey>
    {
        public string Convert(TKey key) => $"{typeof(TKey).Name}-{key.GetSha1()}";
    }
}