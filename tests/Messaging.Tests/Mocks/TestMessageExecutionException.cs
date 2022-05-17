using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Tests.Mocks;

public class TestMessageExecutionException : MessageException
{
    public TestMessageExecutionException() : base("Test exception.") { }

    public TestMessageExecutionException(string message) : base(message) { }
}
