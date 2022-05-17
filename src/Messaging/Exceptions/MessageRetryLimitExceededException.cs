using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     Max retry limit was reached so message handling considered as failed.
/// </summary>
public class MessageRetryLimitExceededException : MessageException
{
    /// <summary/>
    public MessageRetryLimitExceededException() : this("Reached message retry limit.") { }

    /// <summary/>
    public MessageRetryLimitExceededException(string message) : base(message) { }

    /// <summary/>
    public MessageRetryLimitExceededException(string message, Exception ex) : base(message, ex) { }
}