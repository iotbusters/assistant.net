using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public class TestSuccessFailureMessageHandler : IMessageHandler<TestSuccessFailureMessage>
    {
        public Task Handle(TestSuccessFailureMessage message)
        {
            if (message.AssemblyQualifiedExceptionType != null)
            {
                var exceptionType = Type.GetType(message.AssemblyQualifiedExceptionType, throwOnError: true)!;
                throw (Activator.CreateInstance(exceptionType) as Exception)!;
            }
            return Task.CompletedTask;
        }
    }
}