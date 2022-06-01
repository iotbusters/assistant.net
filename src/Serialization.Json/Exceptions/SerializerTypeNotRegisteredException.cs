using System;

namespace Assistant.Net.Serialization.Exceptions;

/// <summary>
///     An exception thrown if no serializer is configured for some type.
/// </summary>
public class SerializerTypeNotRegisteredException : Exception
{
    /// <summary/>
    public SerializerTypeNotRegisteredException(string message) : base(message) { }

    /// <summary/>
    public SerializerTypeNotRegisteredException(Type serializingType) : base($"Type '{serializingType.Name}' wasn't registered.") { }
}
