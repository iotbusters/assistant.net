using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class LocalMessageHandlingProxy<TMessage, TResponse> : IAbstractHandler
    where TMessage : IMessage<TResponse>
{
    private readonly IMessageHandler<TMessage, TResponse> handler;

    public LocalMessageHandlingProxy(IMessageHandler<TMessage, TResponse> handler) =>
        this.handler = handler;

    public async Task<object> Request(object message, CancellationToken token) =>
        (await handler.Handle((TMessage)message, token))!;

    public async Task Publish(object message, CancellationToken token) =>
        // note: it gives a 1ms window to fail the request.
        await await Task.WhenAny(
            handler.Handle((TMessage)message, token),
            Task.Delay(TimeSpan.FromSeconds(0.001), token));
}
