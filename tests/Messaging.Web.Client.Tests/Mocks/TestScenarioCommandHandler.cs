using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public class TestScenarioCommandHandler : ICommandHandler<TestScenarioCommand, TestResponse>
    {
        public Task<TestResponse> Handle(TestScenarioCommand command)
        {
            return command.Scenario switch
            {
                0 => Task.FromResult(new TestResponse(false)),
                1 => throw new InvalidOperationException("1"),
                2 => throw new CommandFailedException("2"),
                3 => throw new CommandFailedException("3", new CommandFailedException("inner")),
                _ => throw new NotImplementedException("Not implemented")
            };
        }
    }
}