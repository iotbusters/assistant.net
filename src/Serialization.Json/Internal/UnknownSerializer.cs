using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Internal
{
    internal class UnknownSerializer<TValue> : ISerializer<TValue>
        where TValue : class
    {
        public Task Serialize(Stream stream, TValue value) =>
            throw new SerializerTypeNotRegisteredException(typeof(TValue));

        public Task<TValue> Deserialize(Stream stream) =>
            throw new SerializerTypeNotRegisteredException(typeof(TValue));
    }
}