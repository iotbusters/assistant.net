using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Message failure exception.
    /// </summary>
    public class MessageFailedException : MessageException
    {
        /// <summary/>
        public MessageFailedException(string message) : base(message) { }

        /// <summary/>
        public MessageFailedException(string message, Exception ex) : base(message, ex) { }

        /// <summary/>
        public MessageFailedException(Exception ex) : this("Message handling has failed.", ex) { }
    }
}