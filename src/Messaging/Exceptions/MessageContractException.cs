using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if remote message handling contract was breached.
/// </summary>
public sealed class MessageContractException : MessageException
{
    /// <summary/>
    public MessageContractException() : this("Remote message handler contract was breached.") { }

    /// <summary/>
    public MessageContractException(string message) : base(message) { }

    /// <summary/>
    public MessageContractException(string message, Exception ex) : base(message, ex) { }
}
