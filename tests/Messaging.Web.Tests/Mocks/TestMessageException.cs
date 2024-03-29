using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public class TestMessageException : MessageException
{
    public TestMessageException() : base("Test exception.") { }

    public TestMessageException(string message) : base(message) { }
}
