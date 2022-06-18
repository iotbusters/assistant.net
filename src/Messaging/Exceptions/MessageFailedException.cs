using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     Message failure exception.
/// </summary>
public sealed class MessageFailedException : MessageException
{
    private const string DefaultMessage = "Message handling has failed.";

    /// <summary/>
    public MessageFailedException() : this(DefaultMessage) { }

    /// <summary/>
    public MessageFailedException(string message) : base(message) { }

    /// <summary/>
    public MessageFailedException(string message, Exception ex) : base(message, ex) { }

    /// <summary/>
    public MessageFailedException(Exception ex) : this(DefaultMessage, ex) { }
}
