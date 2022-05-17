using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     No handlers were registered for message type exception.
/// </summary>
public class MessageNotRegisteredException : MessageException
{
    /// <summary/>
    public MessageNotRegisteredException() : base("Message wasn't registered.") { }

    /// <summary/>
    public MessageNotRegisteredException(string errorMessage) : base(errorMessage) { }

    /// <summary/>
    public MessageNotRegisteredException(Type messageType) : base($"Message '{messageType.Name}' wasn't registered.") { }
}