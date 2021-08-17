namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote handler cannot resolve a received command type exception.
    /// </summary>
    public class CommandNotFoundException : CommandException
    {
        /// <summary/>
        public CommandNotFoundException() : base("Command wasn't found.") { }

        /// <summary/>
        public CommandNotFoundException(string message) : base(message) { }

        /// <summary/>
        public CommandNotFoundException(string message, string commandName) : base($"Command '{commandName}' wasn't found. {message}") { }
    }
}