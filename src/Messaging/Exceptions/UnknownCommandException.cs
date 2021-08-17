using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A wrapper to unknown command exception returned by remote handler.
    /// </summary>
    public sealed class UnknownCommandException : CommandException
    {
        /// <summary/>
        public UnknownCommandException(string exceptionType, string? message, Exception? innerException)
            : base(message, innerException) =>
            Type = exceptionType;

        /// <summary>
        ///     Original exception type which wasn't resolved.
        /// </summary>
        public string Type { get; }
    }
}