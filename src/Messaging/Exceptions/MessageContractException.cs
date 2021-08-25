using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote message handling contract was breached.
    /// </summary>
    public class MessageContractException : MessageException
    {
        /// <summary/>
        public MessageContractException() : this("Remote message handler contract was breached.") { }

        /// <summary/>
        public MessageContractException(string message) : base(message) { }

        /// <summary/>
        public MessageContractException(string message, Exception ex) : base(message, ex) { }
    }
}