using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public abstract class RequestExecutionException : Exception
    {
        protected RequestExecutionException() : base() { }
        protected RequestExecutionException(string message) : base(message) { }
        protected RequestExecutionException(string message, Exception ex) : base(message, ex) { }
        protected RequestExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}