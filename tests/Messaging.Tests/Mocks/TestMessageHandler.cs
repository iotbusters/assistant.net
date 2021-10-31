using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Mocks
{
    public class TestMessageHandler : IMessageHandler<TestMessage, TestResponse>, IMessageHandler<IMessage<object>, object>
    {
        private readonly Func<TestMessage, Task<TestResponse>> handler;

        public TestMessageHandler() : this(DefaultBehavior) { }

        public TestMessageHandler(Exception exception) :this(_=>Task.FromException<TestResponse>(exception)) { }

        public TestMessageHandler(TestResponse response) :this(_=>Task.FromResult(response)) { }

        public TestMessageHandler(Func<TestMessage, Task<TestResponse>> handler) =>
            this.handler = handler;

        public Task<TestResponse> Handle(TestMessage message, CancellationToken token) => handler(message);

        public async Task<object> Handle(IMessage<object> message, CancellationToken token = default) => await Handle((TestMessage)message, token);

        private static Task<TestResponse> DefaultBehavior(TestMessage message)
        {
            return message.Scenario switch
            {
                0 => Task.FromResult(new TestResponse(false)),
                _ => Task.FromException<TestResponse>(new InvalidOperationException("test"))
            };
        }
    }
}
