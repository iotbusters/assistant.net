using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Serialization.Internal
{
    internal class JsonSerializer<TValue> : ISerializer<TValue>
    {
        public TValue Deserialize(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public byte[] Serialize(TValue value)
        {
            throw new System.NotImplementedException();
        }
    }
}