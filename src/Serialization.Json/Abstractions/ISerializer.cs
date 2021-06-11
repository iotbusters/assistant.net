using System.IO;

namespace Assistant.Net.Serialization
{
    public interface ISerializer<TValue>
    {
         byte[] Serialize(TValue value);
         TValue Deserialize(byte[] bytes);
    }
}