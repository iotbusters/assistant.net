using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestCommandHandler : ICommandHandler<TestCommand, TestResponse>
    {
        public Task<TestResponse> Handle(TestCommand command) => command.Scenario switch
        {
            0 => Task.FromResult(new TestResponse(false)),
            _ => Task.FromException<TestResponse>(new InvalidOperationException("test"))
        };
    }
}