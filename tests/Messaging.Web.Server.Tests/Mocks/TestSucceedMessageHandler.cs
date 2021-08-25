using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public class TestSucceedMessageHandler : IMessageHandler<TestSucceedMessage, TestResponse>
    {
        public Task<TestResponse> Handle(TestSucceedMessage message) =>
            Task.FromResult(message.Payload);
    }
}