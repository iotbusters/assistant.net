using System;
using System.Runtime.Serialization;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandRetryLimitExceededException : CommandExecutionException
    {
        public CommandRetryLimitExceededException(Exception ex) : base("Reached command retry limit.", ex) { }
        public CommandRetryLimitExceededException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}