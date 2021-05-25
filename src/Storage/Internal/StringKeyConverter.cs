using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Storage.Internal
{
    public class StringKeyConverter : IKeyConverter<string>
    {
        public string Convert(string key) => key;
    }
}