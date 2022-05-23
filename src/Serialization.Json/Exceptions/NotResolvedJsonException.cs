using System;
using System.Text.Json;

namespace Assistant.Net.Serialization.Exceptions;

/// <summary>
///     An object resolving issue during deserialization.
/// </summary>
public class NotResolvedJsonException : JsonException
{
    /// <summary/>
    public NotResolvedJsonException(string targetType, string message, Exception? innerException) : base(message, innerException) =>
        TargetType = targetType;

    /// <summary>
    ///     Unknown type which wasn't resolved.
    /// </summary>
    public string TargetType { get; }
}
