using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandRetryLimitExceededException : CommandException
    {
        public CommandRetryLimitExceededException(string message) : base(message) { }
        public CommandRetryLimitExceededException(string message, Exception ex) : base(message, ex) { }
        public CommandRetryLimitExceededException(Exception ex) : this("Reached command retry limit.", ex) { }
    }
}