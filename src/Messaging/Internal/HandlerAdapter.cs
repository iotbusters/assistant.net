using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Adapter between de-typed and typed command handlers.
    /// </summary>
    internal class HandlerAdapter<TCommand, TResponse> : IHandlerAdaptorContext, IAbstractHandler, ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private ICommandHandler<TCommand, TResponse>? handler;

        void IHandlerAdaptorContext.Init(IAbstractCommandHandler handler) =>
            this.handler = handler as ICommandHandler<TCommand, TResponse>
                           ?? throw new InvalidOperationException("Unexpected handler type.");

        public Task<TResponse> Handle(TCommand command) =>
            handler?.Handle(command)
            ?? throw new InvalidOperationException($"'{nameof(HandlerAdapter<TCommand, TResponse>)}' wasn't properly initialized.");

        public Task<object> Handle(object command) =>
            Handle((TCommand)command).MapSuccess(x => (object)x!);
    }
}