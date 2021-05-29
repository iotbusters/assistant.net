using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Integration.Tests.Mocks
{
    public class TestSuccessFailureCommandHandler : ICommandHandler<TestSuccessFailureCommand>
    {
        public Task Handle(TestSuccessFailureCommand command)
        {
            if (command.AssemblyQualifiedExceptionType != null)
            {
                var exceptionType = Type.GetType(command.AssemblyQualifiedExceptionType, throwOnError: true)!;
                throw (Activator.CreateInstance(exceptionType) as Exception)!;
            }
            return Task.CompletedTask;
        }
    }
}