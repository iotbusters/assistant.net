using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Sqlite.Tests.Mocks;

public record TestScenarioMessage(int Scenario) : IMessage<TestResponse>;
