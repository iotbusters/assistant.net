namespace Assistant.Net.Messaging.Exceptions
{
    public class RequestRetryLimitExceededException : RequestExecutionException
    {
        public RequestRetryLimitExceededException() : base() { }
    }
}