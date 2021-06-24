namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     An abstraction over binary serializer of <typeparamref name="TValue" />.
    /// </summary>
    /// <typeparam name="TValue">Specific type to serialize.</typeparam>
    public interface ISerializer<TValue>
    {
        /// <summary>
        ///     Serializes <paramref name="value"/> object to binary.
        /// </summary>
        byte[] Serialize(TValue value);

        /// <summary>
        ///     Deserializes binary to <typeparamref name="TValue" /> object.
        /// </summary>
        TValue Deserialize(byte[] bytes);
    }
}