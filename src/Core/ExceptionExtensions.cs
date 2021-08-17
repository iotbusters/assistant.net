using System;
using System.Runtime.ExceptionServices;

namespace Assistant.Net
{
    /// <summary>
    ///     
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        ///     Throws <paramref name="exception"/> with keeping original stack trace.
        /// </summary>
        public static void Throw(this Exception exception) =>
            ExceptionDispatchInfo.Capture(exception).Throw();

        /// <summary>
        ///     Throws <paramref name="exception"/> with keeping original stack trace.
        /// </summary>
        public static T Throw<T>(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            exception.Throw();
            throw exception; // unreachable;
        }
    }
}