using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestCommandHandler1 : ICommandHandler<TestCommand1, TestResponse>
    {
        public Task<TestResponse> Handle(TestCommand1 command)
        {
            if (command.Exception != null)
                throw command.Exception;
            return Task.FromResult(new TestResponse(command.Exception != null));
        }
    }
}