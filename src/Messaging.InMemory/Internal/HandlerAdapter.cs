using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class HandlerAdapter<TCommand, TResponse> : IAbstractHandler, ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> handler;

        public HandlerAdapter(ICommandHandler<TCommand, TResponse> handler) =>
            this.handler = handler;

        public async Task<TResponse> Handle(TCommand command) =>
            await handler.Handle(command);

        public async Task<object> Handle(object command) =>
            await Handle((TCommand)command)
            ?? throw new NotSupportedException("Unexpected null received");
    }
}