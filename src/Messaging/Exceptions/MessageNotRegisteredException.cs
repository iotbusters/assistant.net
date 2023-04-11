using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if no handlers were registered for message type.
/// </summary>
public sealed class MessageNotRegisteredException : MessageException
{
    /// <summary/>
    public MessageNotRegisteredException() : base("Message wasn't registered.") { }

    /// <summary/>
    public MessageNotRegisteredException(string errorMessage) : base(errorMessage) { }

    /// <summary/>
    public MessageNotRegisteredException(Type messageType) : base($"Message '{messageType}' wasn't registered.") { }
}
