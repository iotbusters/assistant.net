namespace Assistant.Net.Messaging.Exceptions
{
    public sealed class NoneCommandException : CommandException
    {
        public NoneCommandException(string message) : base(message){}
    }
}