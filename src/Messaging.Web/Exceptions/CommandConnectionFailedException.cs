using Assistant.Net.Messaging.Exceptions;

namespace Assistance.Net.Messaging.Web.Exceptions
{
    public class CommandConnectionFailedException : CommandException
    {
        public CommandConnectionFailedException() : this("Connection to remote command handler has failed.") { }
        public CommandConnectionFailedException(string message) : base(message) { }
    }
}