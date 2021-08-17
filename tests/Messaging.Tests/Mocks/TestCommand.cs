using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public record TestCommand(int Scenario) : ICommand<TestResponse>;
}