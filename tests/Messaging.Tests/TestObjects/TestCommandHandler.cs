using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests.TestObjects
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