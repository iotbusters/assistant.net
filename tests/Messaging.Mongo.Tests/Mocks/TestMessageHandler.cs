using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks
{
    public class TestMessageHandler<TMessage, TResponse> : IMessageHandler<TMessage, TResponse> where TMessage : IMessage<TResponse>
    {
        private readonly TResponse response;

        public TestMessageHandler(TResponse response) =>
            this.response = response;

        public Task<TResponse> Handle(TMessage message, CancellationToken token)
        {
            Message = message;
            return Task.FromResult(response);
        }

        public TMessage? Message { get; set; }
    }
}
