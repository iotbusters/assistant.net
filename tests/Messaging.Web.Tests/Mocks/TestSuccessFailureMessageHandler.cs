using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public class TestSuccessFailureMessageHandler : IMessageHandler<TestSuccessFailureMessage>
{
    public Task Handle(TestSuccessFailureMessage message, CancellationToken token)
    {
        if (message.AssemblyQualifiedExceptionType != null)
        {
            var exceptionType = Type.GetType(message.AssemblyQualifiedExceptionType, throwOnError: true)!;
            throw (Activator.CreateInstance(exceptionType) as Exception)!;
        }
        return Task.CompletedTask;
    }
}
