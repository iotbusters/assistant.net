using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public record TestCommand1(int Scenario) : ICommand<TestResponse>;
}