using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Internal
{
    internal class UnknownSerializer<TValue> : ISerializer<TValue>
        where TValue : class
    {
        public Task Serialize(Stream stream, TValue value, CancellationToken token) =>
            throw new SerializerTypeNotRegisteredException(typeof(TValue));

        public Task<TValue> Deserialize(Stream stream, CancellationToken token) =>
            throw new SerializerTypeNotRegisteredException(typeof(TValue));
    }
}