namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Any issues connecting to remote command handler.
    /// </summary>
    public class CommandConnectionFailedException : CommandException
    {
        /// <summary/>
        public CommandConnectionFailedException() : this("Connection to remote command handler has failed.") { }

        /// <summary/>
        public CommandConnectionFailedException(string message) : base(message) { }
    }
}