using Assistant.Net.Storage.Abstractions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters
{
    internal class StringKeyConverter : IKeyConverter<string>
    {
        public Task<byte[]> Convert(string key, CancellationToken _) => Task.FromResult(Encoding.UTF8.GetBytes(key));
        
        public Task<string> Convert(byte[] keyContent, CancellationToken _) => Task.FromResult(Encoding.UTF8.GetString(keyContent));
    }
}