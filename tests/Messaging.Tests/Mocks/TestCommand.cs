using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public record TestMessage(int Scenario) : IMessage<TestResponse>;
}