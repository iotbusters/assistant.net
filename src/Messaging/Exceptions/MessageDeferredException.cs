namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     Remote message handler cannot return response temporary.
/// </summary>
public sealed class MessageDeferredException : MessageException
{
    /// <summary/>
    public MessageDeferredException() : base("Remote message handler deferred a message.") { }

    /// <summary/>
    public MessageDeferredException(string message) : base(message) { }
}