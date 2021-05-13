using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed web proxy to remote handling.
    /// </summary>
    internal class CommandHandlerWebProxy<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly RemoteCommandHandlingClient client;

        public CommandHandlerWebProxy(RemoteCommandHandlingClient client) =>
            this.client = client;

        public async Task<TResponse> Handle(TCommand command) =>
            await client.DelegateHandling(command);
    }
}