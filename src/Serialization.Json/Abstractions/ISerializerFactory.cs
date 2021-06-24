using System;

namespace Assistant.Net.Serialization.Abstractions
{
    /// <summary>
    ///     An abstraction over binary serializer factory.
    /// </summary>
    public interface ISerializerFactory
    {
        /// <summary>
        ///     Creates a serializer of <paramref name="serializingType"/>.
        /// </summary>
        ISerializer<object> Create(Type serializingType);

        /// <summary>
        ///     Creates a serializer of <typeparamref name="TValue"/>.
        /// </summary>
        ISerializer<TValue> Create<TValue>();
    }
}