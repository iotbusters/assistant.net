using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandUnknownException : CommandException
    {
        public CommandUnknownException(Type commandType) : base($"Command {commandType} wasn't registered.") { }
        public CommandUnknownException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}