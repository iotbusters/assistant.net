namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Any issues connecting to remote message handler.
    /// </summary>
    public class MessageConnectionFailedException : MessageException
    {
        /// <summary/>
        public MessageConnectionFailedException() : this("Connection to remote message handler has failed.") { }

        /// <summary/>
        public MessageConnectionFailedException(string message) : base(message) { }
    }
}