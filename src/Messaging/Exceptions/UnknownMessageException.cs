using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A wrapper to unknown message exception returned by remote handler.
    /// </summary>
    public sealed class UnknownMessageException : MessageException
    {
        /// <summary/>
        public UnknownMessageException(string exceptionType, string? errorMessage, Exception? innerException)
            : base(errorMessage, innerException) =>
            Type = exceptionType;

        /// <summary>
        ///     Original exception type which wasn't resolved.
        /// </summary>
        public string Type { get; }
    }
}