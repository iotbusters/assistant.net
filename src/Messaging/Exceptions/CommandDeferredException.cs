namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandDeferredException : CommandException
    {
        public CommandDeferredException() : base("Remote command handler deferred a command.") { }
        public CommandDeferredException(string message) : base(message) { }
    }
}