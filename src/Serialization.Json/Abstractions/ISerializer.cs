namespace Assistant.Net.Serialization.Abstractions
{
    public interface ISerializer<TValue>
    {
         byte[] Serialize(TValue value);
         TValue Deserialize(byte[] bytes);
    }
}