using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command handler abstraction that accepts <typeparamref name="TCommand" /> only and returns <typeparam name="TResponse" /> in response.
    /// </summary>
    public interface ICommandHandler<TCommand, TResponse> : IAbstractHandler
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        ///     Handles <typeparam name="TCommand" /> object.
        /// </summary>
        Task<TResponse> Handle(TCommand command);

        Task<object> IAbstractHandler.Handle(object command) => Handle((TCommand) command).MapSuccess(x => (object) x!);
    }

    /// <summary>
    ///     Command handler abstraction that accepts <typeparam name="TCommand" /> only when no object in response is expected.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, None>
        where TCommand : ICommand
    {
        new Task Handle(TCommand command);

        async Task<None> ICommandHandler<TCommand, None>.Handle(TCommand command)
        {
            await Handle(command);
            return None.Instance;
        }
    }
}