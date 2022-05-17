using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks;

public class TestFailMessageHandler : IMessageHandler<TestFailMessage>
{
    public Task Handle(TestFailMessage message, CancellationToken token)
    {
        if (message.AssemblyQualifiedExceptionTypeName == null)
            return Task.CompletedTask;

        var type = Type.GetType(message.AssemblyQualifiedExceptionTypeName, throwOnError: true)!;
        var exception = Activator.CreateInstance(type) as Exception;
        return Task.FromException(exception!);
    }
}
