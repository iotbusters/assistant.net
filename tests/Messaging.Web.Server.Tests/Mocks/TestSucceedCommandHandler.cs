using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public class TestSucceedCommandHandler : ICommandHandler<TestSucceedCommand, TestResponse>
    {
        public Task<TestResponse> Handle(TestSucceedCommand command) =>
            Task.FromResult(command.Payload);
    }
}