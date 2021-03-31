using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Web.Client;

namespace Assistant.Net.Messaging.Internal
{
    public class CommandHandlerWebProxy<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly RemoteCommandHandlingClient client;

        public CommandHandlerWebProxy(RemoteCommandHandlingClient client) =>
            this.client = client;

        public async Task<TResponse> Handle(TCommand command) =>
            await client.DelegateHandling(command);
    }
}