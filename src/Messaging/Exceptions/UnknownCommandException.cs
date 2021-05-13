using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A wrapper to unknown command exception returned by remote handler.
    /// </summary>
    public sealed class UnknownCommandException : CommandException
    {
        public UnknownCommandException(string exceptionType, string? message, Exception? innerException)
            : base(message, innerException) =>
            Type = exceptionType;

        public string Type { get; }
    }
}