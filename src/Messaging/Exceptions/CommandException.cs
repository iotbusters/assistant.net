using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A parent to command exception family.
    /// </summary>
    public abstract class CommandException : Exception
    {
        /// <summary>
        ///     Pay attention, this ctor is required to implement for all children.
        ///     Otherwise, deserialization will fail.
        /// </summary>
        protected CommandException(string? message) : base(message) { }
        protected CommandException(string? message, Exception? ex) : base(message, ex) { }
    }
}