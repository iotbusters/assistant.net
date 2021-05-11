using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandContractException : CommandException
    {
        public CommandContractException() : this("Remote command handler contract breached.") { }
        public CommandContractException(string message) : base(message) { }
        public CommandContractException(string message, Exception ex) : base(message, ex) { }
    }
}