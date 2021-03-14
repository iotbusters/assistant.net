namespace Assistant.Net.Messaging.Exceptions
{
    public class RequestFailedException : RequestExecutionException
    {
        public RequestFailedException(string message) : base(message) { }
    }
}