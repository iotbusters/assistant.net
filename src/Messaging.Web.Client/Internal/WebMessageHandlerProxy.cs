using Assistant.Net.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed proxy to remote message handling.
    /// </summary>
    internal class WebMessageHandlerProxy<TMessage, TResponse> : IMessageHandler<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly IWebMessageHandlerClient client;

        public WebMessageHandlerProxy(IWebMessageHandlerClient client) =>
            this.client = client;

        public Task<TResponse> Handle(TMessage message, CancellationToken token) =>
            client.DelegateHandling(message, token);
    }
}
