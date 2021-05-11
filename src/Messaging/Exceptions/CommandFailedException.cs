using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandFailedException : CommandException
    {
        public CommandFailedException(string message) : base(message) { }
        public CommandFailedException(string message, Exception ex) : base(message, ex) { }
        public CommandFailedException(Exception ex) : this("Command execution has failed.", ex) { }
    }
}