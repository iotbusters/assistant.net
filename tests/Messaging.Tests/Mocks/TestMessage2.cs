using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.Mocks;

public record TestMessage2(int Scenario) : IMessage<TestResponse2>;
