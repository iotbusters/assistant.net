using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    internal class AbstractHandler<TMessage, TResponse> : IAbstractHandler where TMessage : IMessage<TResponse>
    {
        private readonly IMessageHandler<TMessage, TResponse> handler;

        public AbstractHandler(IServiceProvider provider)
        {
            handler = provider.GetService<IMessageHandler<TMessage, TResponse>>()
                      ?? throw new MessageNotRegisteredException(typeof(TMessage));
        }

        public async Task<object> Handle(object message, CancellationToken token) =>
            (await handler.Handle((TMessage)message, token))!;
    }
}
