namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote handler cannot resolve a received command type exception.
    /// </summary>
    public class CommandNotFoundException : CommandException
    {
        public CommandNotFoundException(string message) : base(message) { }
    }
}