using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command handler abstraction that accepts <typeparamref name="TCommand" /> only and returns <typeparamref name="TResponse" /> in response.
    /// </summary>
    /// <typeparam name="TCommand">Specific command implementation type.</typeparam>
    /// <typeparam name="TResponse">Response type of <typeparamref name="TCommand"/>.</typeparam>
    public interface ICommandHandler<in TCommand, TResponse> : IAbstractHandler
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        ///     Handles <typeparamref name="TCommand" /> object.
        /// </summary>
        Task<TResponse> Handle(TCommand command);

        Task<object> IAbstractHandler.Handle(object command) => Handle((TCommand) command).MapSuccess(x => (object) x!);
    }

    /// <summary>
    ///     Command handler abstraction that accepts <typeparamref name="TCommand" /> only when no object in response is expected.
    /// </summary>
    /// <typeparam name="TCommand">Specific command implementation type.</typeparam>
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, None>
        where TCommand : ICommand
    {
        /// <summary>
        ///     Handles <typeparamref name="TCommand" /> object.
        /// </summary>
        new Task Handle(TCommand command);

        async Task<None> ICommandHandler<TCommand, None>.Handle(TCommand command)
        {
            await Handle(command);
            return None.Instance;
        }
    }
}