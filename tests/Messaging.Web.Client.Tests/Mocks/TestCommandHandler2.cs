using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public class TestCommandHandler2 : ICommandHandler<TestCommand2>
    {
        public Task Handle(TestCommand2 command)
        {
            if (command.Exception != null)
                throw command.Exception;
            return Task.CompletedTask;
        }
    }
}