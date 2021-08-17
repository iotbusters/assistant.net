using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestCommandExecutionException : CommandException
    {
        public TestCommandExecutionException() : base("Test exception.") { }
    }
}