using System;

namespace Assistant.Net.Serialization.Exceptions;

/// <summary>
///     Exception resolving issue during deserialization.
/// </summary>
public class ExceptionNotResolvedJsonException : NotResolvedJsonException
{
    /// <summary/>
    public ExceptionNotResolvedJsonException(
        string message,
        Exception? innerException,
        string targetType,
        string targetMessage,
        Exception? targetInner) : base(targetType, message, innerException)
    {
        TargetMessage = targetMessage;
        TargetInner = targetInner;
    }

    /// <summary>
    ///     Message of unknown exception.
    /// </summary>
    public string TargetMessage { get; }

    /// <summary>
    ///     Inner exception of unknown exception.
    /// </summary>
    public Exception? TargetInner { get; }
}
