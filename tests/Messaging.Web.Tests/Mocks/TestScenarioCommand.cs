using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public record TestScenarioMessage(int Scenario) : IMessage<TestResponse>;
