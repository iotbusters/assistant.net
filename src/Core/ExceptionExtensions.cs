using System;
using System.Diagnostics;
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
    [StackTraceHidden]
    public static void Throw(this Exception exception)
    {
        // todo: filter out stack trace from System and Microsoft internals.
        ExceptionDispatchInfo.Capture(exception).Throw();
    }

    /// <summary>
    ///     Throws <paramref name="exception"/> with keeping original stack trace.
    /// </summary>
    /// <exception cref="Exception"/>
    [StackTraceHidden]
    public static T Throw<T>(this Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        exception.Throw();
        throw exception; // unreachable;
    }
}
