using System;
using System.Runtime.ExceptionServices;

namespace Assistant.Net;

/// <summary>
///     Exception throwing extensions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    ///     Throws <paramref name="exception"/> with keeping original stack trace.
    /// </summary>
    /// <exception cref="Exception"/>
    public static void Throw(this Exception exception) =>
        ExceptionDispatchInfo.Capture(exception).Throw();

    /// <summary>
    ///     Throws <paramref name="exception"/> with keeping original stack trace.
    /// </summary>
    /// <exception cref="Exception"/>
    public static T Throw<T>(this Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        exception.Throw();
        throw exception; // unreachable;
    }
}