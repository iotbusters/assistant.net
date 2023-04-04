using System;

namespace Assistant.Net.Serialization.Exceptions;

/// <summary>
///     An exception thrown if no specific format serializer is configured.
/// </summary>
public class SerializerNotRegisteredException : Exception
{
    /// <summary/>
    public SerializerNotRegisteredException(string message) : base(message) { }

    /// <summary/>
    public SerializerNotRegisteredException() : base("Single format serializer wasn't registered.") { }
}
