namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if no message type was found by name.
/// </summary>
public sealed class MessageNotFoundException : MessageException
{
    /// <summary/>
    public MessageNotFoundException() : base("Message wasn't found.") { }

    /// <summary/>
    public MessageNotFoundException(string errorMessage) : base(errorMessage) { }

    /// <summary/>
    public MessageNotFoundException(string errorMessage, string messageName) : base($"Message '{messageName}' wasn't found. {errorMessage}") { }
}
