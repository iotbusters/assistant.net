using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public class TestScenarioMessageHandler : IMessageHandler<TestScenarioMessage, TestResponse>
    {
        public Task<TestResponse> Handle(TestScenarioMessage message, CancellationToken token)
        {
            return message.Scenario switch
            {
                0 => Task.FromResult(new TestResponse(false)),
                1 => throw new InvalidOperationException("1"),
                2 => throw new MessageFailedException("2"),
                3 => throw new MessageFailedException("3", new MessageFailedException("inner")),
                _ => throw new NotImplementedException("Not implemented")
            };
        }
    }
}