using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class HandlerAdaptor<TCommand, TResponse> : IAbstractHandler, ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> handler;

        public HandlerAdaptor(ICommandHandler<TCommand, TResponse> handler) =>
            this.handler = handler;

        public Task<TResponse> Handle(TCommand command) =>
            handler.Handle(command);

        public async Task<object> Handle(object command) =>
            await Handle((TCommand)command);
    }
}