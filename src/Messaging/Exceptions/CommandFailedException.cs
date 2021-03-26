using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandFailedException : CommandExecutionException
    {
        public CommandFailedException(string message) : base(message) { }
        public CommandFailedException(Exception ex) : base("Command execution has failed.", ex) { }
        public CommandFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}