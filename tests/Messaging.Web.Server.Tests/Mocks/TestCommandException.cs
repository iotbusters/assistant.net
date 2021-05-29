using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public class TestCommandException : CommandException
    {
        public TestCommandException() : base("Test exception.") { }

        public TestCommandException(string message) : base(message) { }
    }
}