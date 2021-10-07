using System;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Execution result for caching mechanism.
    /// </summary>
    public abstract class CachingResult
    {
        /// <summary>
        ///     Gets cached result.
        /// </summary>
        public abstract object GetValue();

        /// <summary>
        ///     Creates instance of <see cref="CachingResult"/> with a value.
        /// </summary>
        public static CachingValueResult<T> OfValue<T>(T value) => new(value);

        /// <summary>
        ///     Creates instance of <see cref="CachingResult"/> with an exception.
        /// </summary>
        public static CachingExceptionResult OfException(Exception exception) => new(exception);
    }
}
