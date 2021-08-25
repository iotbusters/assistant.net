namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote handler cannot resolve a received message type exception.
    /// </summary>
    public class MessageNotFoundException : MessageException
    {
        /// <summary/>
        public MessageNotFoundException() : base("Message wasn't found.") { }

        /// <summary/>
        public MessageNotFoundException(string errorMessage) : base(errorMessage) { }

        /// <summary/>
        public MessageNotFoundException(string errorMessage, string messageName) : base($"Message '{messageName}' wasn't found. {errorMessage}") { }
    }
}