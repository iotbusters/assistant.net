using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Common failure command exception.
    /// </summary>
    public class CommandFailedException : CommandException
    {
        /// <summary/>
        public CommandFailedException(string message) : base(message) { }

        /// <summary/>
        public CommandFailedException(string message, Exception ex) : base(message, ex) { }

        /// <summary/>
        public CommandFailedException(Exception ex) : this("Command execution has failed.", ex) { }
    }
}