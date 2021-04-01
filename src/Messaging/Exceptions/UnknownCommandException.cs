using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public sealed class UnknownCommandException : CommandException
    {
        public UnknownCommandException(string exceptionType, string? message, Exception? innerException)
            : base(message, innerException) =>
            Type = exceptionType;

        public string Type { get; }
    }
}