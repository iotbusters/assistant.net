using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public record TestSucceedCommand(TestResponse Payload) : ICommand<TestResponse>;
}