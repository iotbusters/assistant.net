using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Mocks
{
    public class TestFailCommandHandler : ICommandHandler<TestFailCommand>
    {
        public Task Handle(TestFailCommand command)
        {
            if (command.AssemblyQualifiedExceptionTypeName == null)
                return Task.CompletedTask;

            var type = Type.GetType(command.AssemblyQualifiedExceptionTypeName, throwOnError: true)!;
            var exception = Activator.CreateInstance(type) as Exception;
            return Task.FromException(exception!);
        }
    }
}