namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote command handler cannot return response temporary.
    /// </summary>
    public class CommandDeferredException : CommandException
    {
        /// <summary/>
        public CommandDeferredException() : base("Remote command handler deferred a command.") { }

        /// <summary/>
        public CommandDeferredException(string message) : base(message) { }
    }
}