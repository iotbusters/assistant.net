using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     Any connection related issues occurred during remote message handling.
/// </summary>
public class MessageConnectionFailedException : MessageException
{
    /// <summary/>
    public MessageConnectionFailedException() : this("Connection to remote message handler has failed.") { }

    /// <summary/>
    public MessageConnectionFailedException(string message) : base(message) { }

    /// <summary/>
    public MessageConnectionFailedException(string? message, Exception? ex) : base(message, ex) { }
}