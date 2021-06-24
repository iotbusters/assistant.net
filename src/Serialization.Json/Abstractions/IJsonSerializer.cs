using System;

namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     An abstraction over common purpose JSON serializer.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        ///     Serializes <paramref name="value"/> object to binary JSON.
        /// </summary>
        byte[] Serialize(object value);

        /// <summary>
        ///     Deserializes binary JSON to <paramref name="type" /> object.
        /// </summary>
        object Deserialize(byte[] bytes, Type type);
    }
}