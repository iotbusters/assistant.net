using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestMessageHandler : IMessageHandler<TestMessage, TestResponse>
    {
        public Task<TestResponse> Handle(TestMessage message, CancellationToken token) => message.Scenario switch
        {
            0 => Task.FromResult(new TestResponse(false)),
            _ => Task.FromException<TestResponse>(new InvalidOperationException("test"))
        };
    }
}