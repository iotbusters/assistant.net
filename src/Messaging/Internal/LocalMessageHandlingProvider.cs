using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    internal class LocalMessageHandlingProvider<TMessage, TResponse> : IMessageHandlingProvider<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly IMessageHandler<TMessage, TResponse> handler;

        public LocalMessageHandlingProvider(IServiceProvider provider)
        {
            handler = provider.GetService<IMessageHandler<TMessage, TResponse>>()
                      ?? throw new MessageNotRegisteredException(typeof(TMessage));
        }

        public async Task<object> Request(object message, CancellationToken token) =>
            (await handler.Handle((TMessage)message, token))!;

        public async Task Publish(object message, CancellationToken token) =>
            // note: it gives a 1ms window to fail the request.
            await await Task.WhenAny(
                handler.Handle((TMessage)message, token),
                Task.Delay(TimeSpan.FromSeconds(0.001), token));
    }
}
