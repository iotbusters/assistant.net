using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Internal
{
    internal class StringKeyConverter : IKeyConverter<string>
    {
        public string Convert(string key) => key;
    }
}