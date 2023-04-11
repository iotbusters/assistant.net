using System;

namespace Assistant.Net.Messaging.Exceptions;

/// <summary>
///     The exception thrown if unknown <see cref="MessageException"/> received.
/// </summary>
public sealed class UnknownMessageException : MessageException
{
    /// <summary/>
    public UnknownMessageException(string exceptionType, string? errorMessage, Exception? innerException)
        : base(errorMessage, innerException) =>
        ExceptionType = exceptionType;

    /// <summary>
    ///     Original exception type which wasn't resolved.
    /// </summary>
    public string ExceptionType { get; }
}
