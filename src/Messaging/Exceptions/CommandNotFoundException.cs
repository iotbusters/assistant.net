namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandNotFoundException : CommandException
    {
        public CommandNotFoundException(string message) : base(message) { }
    }
}