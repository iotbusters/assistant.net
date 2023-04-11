using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if no single handler was registered.
/// </summary>
public class HandlerNotRegisteredException : MessageException
{
    /// <summary/>
    public HandlerNotRegisteredException() : this("Single message handler wasn't registered.") { }

    /// <summary/>
    public HandlerNotRegisteredException(string? message) : base(message) { }

    /// <summary/>
    public HandlerNotRegisteredException(string? message, Exception? ex) : base(message, ex) { }
}
