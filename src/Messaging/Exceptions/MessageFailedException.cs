using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if some generic message failure occurred.
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
