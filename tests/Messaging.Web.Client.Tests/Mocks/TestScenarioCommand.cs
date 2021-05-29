using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public record TestScenarioCommand(int Scenario) : ICommand<TestResponse>;
}