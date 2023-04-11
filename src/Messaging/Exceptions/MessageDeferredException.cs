using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if remote message handler cannot temporary respond.
/// </summary>
public sealed class MessageDeferredException : MessageException
{
    /// <summary/>
    public MessageDeferredException() : base("Remote message handler deferred a message.") { }

    /// <summary/>
    public MessageDeferredException(string message) : base(message) { }

    /// <summary/>
    public MessageDeferredException(string message, Exception exception) : base(message, exception) { }
}
