using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging
{
    internal class CommandClient : ICommandClient
    {
        private readonly IHandlerFactory factory;

        public CommandClient(IHandlerFactory factory) =>
            this.factory = factory;

        public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command)
        {
            var handler = factory.Create(command.GetType());
            return (TResponse)await handler.Handle(command);
        }
    }
}