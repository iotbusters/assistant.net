using System;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Exception result for caching mechanism.
    /// </summary>
    public sealed class CachingExceptionResult : CachingResult
    {
        /// <summary/>
        public CachingExceptionResult(Exception exception) =>
            Exception = exception;

        /// <summary>
        ///     Failed result exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     Throws occurred exception.
        /// </summary>
        public override object GetValue() => Exception.Throw<object>();
    }
}
