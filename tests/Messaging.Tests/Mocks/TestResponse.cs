namespace Assistant.Net.Messaging.Tests.Mocks;

public record TestResponse(bool Fail)
{
    public TestResponse() : this(false) { }
}
