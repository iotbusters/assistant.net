using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Tests.TestObjects
{
    public class TestCommandHandler1 : ICommandHandler<TestCommand1, TestResponse>
    {
        public Task<TestResponse> Handle(TestCommand1 command)
        {
            if(command.Fail)
                throw new CommandFailedException(nameof(TestCommandHandler1));
            return Task.FromResult(new TestResponse(command.Fail));
        }
    }
    public class TestCommandHandler2 : ICommandHandler<TestCommand2>
    {
        public Task Handle(TestCommand2 command)
        {
            if(command.Fail)
                throw new CommandFailedException(nameof(TestCommandHandler2));
            return Task.FromResult(new TestResponse(command.Fail));
        }
    }
}