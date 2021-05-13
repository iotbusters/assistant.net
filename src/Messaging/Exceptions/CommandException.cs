using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A parent to command exception family.
    /// </summary>
    public abstract class CommandException : Exception
    {
        protected CommandException() : base() { }
        protected CommandException(string? message) : base(message) { }
        protected CommandException(string? message, Exception? ex) : base(message, ex) { }
    }
}