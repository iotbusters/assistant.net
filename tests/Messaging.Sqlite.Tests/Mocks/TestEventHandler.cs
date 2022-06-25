using Assistant.Net.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Sqlite.Tests.Mocks;

public class TestEventHandler<TMessage> : IMessageHandler<TMessage> where TMessage : IMessage
{
    private readonly Func<TMessage, Task> handle;

    public TestEventHandler(Func<TMessage, Task> handle)
    {
        this.handle = handle;
    }

    public TestEventHandler(Action<TMessage> handle) : this(x =>
    {
        handle(x);
        return Task.CompletedTask;
    }) { }

    public TestEventHandler() : this(delegate { }) { }

    public IList<TMessage> Messages { get; } = new List<TMessage>();

    public Task Handle(TMessage message, CancellationToken token)
    {
        Messages.Add(message);
        return handle(message);
    }
}
