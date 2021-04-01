using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandConnectionFailedException : CommandException
    {
        public CommandConnectionFailedException() : this("Connection to remote command handler has failed.") { }
        public CommandConnectionFailedException(string message) : base(message) { }
    }
}