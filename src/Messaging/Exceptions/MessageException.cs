using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     A base exception to the whole message related failures.
/// </summary>
public abstract class MessageException : Exception
{
    /// <remarks>
    ///     Pay attention, this ctor is required to implement for all derived exception types.
    ///     Otherwise, deserialization may fail.
    /// </remarks>
    protected MessageException(string? message) : base(message) { }

    /// <summary/>
    protected MessageException(string? message, Exception? ex) : base(message, ex) { }
}
