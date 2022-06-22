﻿using Assistant.Net.Messaging.Abstractions;
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
        await handler.Handle((TMessage)message, token);
}
