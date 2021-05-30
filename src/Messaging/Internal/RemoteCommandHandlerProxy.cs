using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed proxy to remote handling.
    /// </summary>
    internal class RemoteCommandHandlerProxy<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly IRemoteCommandClient client;

        public RemoteCommandHandlerProxy(IRemoteCommandClient client) =>
            this.client = client;

        public async Task<TResponse> Handle(TCommand command) =>
            await client.DelegateHandling(command);
    }
}