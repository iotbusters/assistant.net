using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Remote command handling contract was breached.
    /// </summary>
    public class CommandContractException : CommandException
    {
        /// <summary/>
        public CommandContractException() : this("Remote command handler contract was breached.") { }

        /// <summary/>
        public CommandContractException(string message) : base(message) { }

        /// <summary/>
        public CommandContractException(string message, Exception ex) : base(message, ex) { }
    }
}