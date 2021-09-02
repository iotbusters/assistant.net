using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     A parent to message exception family.
    /// </summary>
    public abstract class MessageException : Exception
    {
        /// <summary>
        ///     Pay attention, this ctor is required to implement for all children.
        ///     Otherwise, deserialization will fail.
        /// </summary>
        protected MessageException(string? message) : base(message) { }

        /// <summary/>
        protected MessageException(string? message, Exception? ex) : base(message, ex) { }
    }
}