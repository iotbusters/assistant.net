using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Mongo.Tests.Mocks;

public class TestHostSelectionStrategy : IHostSelectionStrategy
{
    public Func<Type, string?> InstanceGetter { private get; set; } = delegate
    {
        throw new InvalidOperationException("Server availability wasn't arranged.");
    };

    public Task<string?> GetInstance(Type messageType, CancellationToken _) =>
        Task.FromResult(InstanceGetter(messageType));
}
