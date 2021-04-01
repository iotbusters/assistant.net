using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public abstract class CommandException : Exception
    {
        protected CommandException() : base() { }
        protected CommandException(string? message) : base(message) { }
        protected CommandException(string? message, Exception? ex) : base(message, ex) { }
    }
}