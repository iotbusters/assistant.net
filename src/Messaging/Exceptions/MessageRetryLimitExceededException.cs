using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if max retry limit was reached.
/// </summary>
public sealed class MessageRetryLimitExceededException : MessageException
{
    /// <summary/>
    public MessageRetryLimitExceededException() : this("Reached message retry limit.") { }

    /// <summary/>
    public MessageRetryLimitExceededException(string message) : base(message) { }

    /// <summary/>
    public MessageRetryLimitExceededException(string message, Exception ex) : base(message, ex) { }
}
