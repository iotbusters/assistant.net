using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     Race condition issues occurred to remote message handler in concurrent environment.
/// </summary>
public sealed class MessageConcurrencyException : MessageException
{
    /// <summary/>
    public MessageConcurrencyException() : this("Message handling failed in concurrent environment.") { }

    /// <summary/>
    public MessageConcurrencyException(string? message) : base(message) { }

    /// <summary/>
    public MessageConcurrencyException(string? message, Exception? ex) : base(message, ex) { }
}