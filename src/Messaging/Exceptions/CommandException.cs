using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public abstract class CommandException : Exception
    {
        protected CommandException() : base() { }
        protected CommandException(string message) : base(message) { }
        protected CommandException(string message, Exception ex) : base(message, ex) { }
        protected CommandException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}