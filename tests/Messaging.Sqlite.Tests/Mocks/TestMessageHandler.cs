using Assistant.Net.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests.Mocks;

public class TestMessageHandler<TMessage, TResponse> : IMessageHandler<TMessage, TResponse> where TMessage : IMessage<TResponse>
{
    private readonly Func<TMessage, Task<TResponse>> handle;

    public TestMessageHandler(Func<TMessage,Task<TResponse>> handle) =>
        this.handle = handle;

    public TestMessageHandler(Func<TMessage, TResponse> handle) : this(x => Task.FromResult(handle(x))) { }

    public TestMessageHandler(TResponse response) : this(_ => response) { }

    public Task<TResponse> Handle(TMessage message, CancellationToken token)
    {
        Messages.Add(message);
        return handle(message);
    }

    public IList<TMessage> Messages { get; } = new List<TMessage>();
}
