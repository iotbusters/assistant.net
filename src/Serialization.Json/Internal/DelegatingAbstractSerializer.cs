using System;
using System.IO;
using System.Threading.Tasks;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class DelegatingAbstractSerializer : ISerializer<object>
    {
        private readonly Func<Stream, object, Task> serialize;
        private readonly Func<Stream, Task<object>> deserialize;

        public DelegatingAbstractSerializer(Func<Stream, object, Task> serialize, Func<Stream, Task<object>> deserialize)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public Task Serialize(Stream stream, object value) => serialize(stream, value);

        public Task<object> Deserialize(Stream stream) => deserialize(stream);
    }
}