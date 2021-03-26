using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public abstract class CommandExecutionException : Exception
    {
        protected CommandExecutionException() : base() { }
        protected CommandExecutionException(string message) : base(message) { }
        protected CommandExecutionException(string message, Exception ex) : base(message, ex) { }
        protected CommandExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}