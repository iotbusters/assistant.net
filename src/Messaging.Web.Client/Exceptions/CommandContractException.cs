using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandContractException : CommandException
    {
        public CommandContractException() : this("Remote command handler contract breach.") { }
        public CommandContractException(string message) : base(message) { }
    }
}