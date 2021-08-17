using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     Max retry limit was reached so command handling considered as failed.
    /// </summary>
    public class CommandRetryLimitExceededException : CommandException
    {
        /// <summary/>
        public CommandRetryLimitExceededException() : this("Reached command retry limit.") { }

        /// <summary/>
        public CommandRetryLimitExceededException(string message) : base(message) { }

        /// <summary/>
        public CommandRetryLimitExceededException(string message, Exception ex) : base(message, ex) { }
    }
}