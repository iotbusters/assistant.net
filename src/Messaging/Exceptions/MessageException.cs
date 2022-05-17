using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     A parent to message exception family.
/// </summary>
public abstract class MessageException : Exception
{
    /// <remarks>
    ///     Pay attention, this ctor is required to implement for all children.
    ///     Otherwise, deserialization will fail.
    /// </remarks>
    protected MessageException(string? message) : base(message) { }

    /// <summary/>
    protected MessageException(string? message, Exception? ex) : base(message, ex) { }
}