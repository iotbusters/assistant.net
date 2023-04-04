using System;

namespace Assistant.Net.Serialization.Exceptions;

/// <summary>
///     An exception thrown if no serializer is configured for some type.
/// </summary>
public sealed class SerializingTypeNotRegisteredException : Exception
{
    /// <summary/>
    public SerializingTypeNotRegisteredException(string message) : base(message) { }

    /// <summary/>
    public SerializingTypeNotRegisteredException(Type serializingType) : base($"Type '{serializingType.FullName}' wasn't registered.") { }
}
