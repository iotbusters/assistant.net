using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestCommandHandler2 : ICommandHandler<TestCommand2>
    {
        public Task Handle(TestCommand2 command)
        {
            if (command.Exception != null)
                throw command.Exception;
            return Task.CompletedTask;
        }
    }
}