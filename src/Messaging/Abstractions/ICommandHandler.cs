using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command handler abstraction that accepts <typeparamref name="TCommand" /> only and returns <typeparam name="TResponse" /> in response.
    /// </summary>
    public interface ICommandHandler<TCommand, TResponse> : IAbstractCommandHandler where TCommand : ICommand<TResponse>
    {
        /// <summary>
        ///     Handles <typeparam name="TCommand" /> object.
        /// </summary>
        Task<TResponse> Handle(TCommand command);
    }

    /// <summary>
    ///     Command handler abstraction that accepts <typeparam name="TCommand" /> only when no object in response is expected.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, None> where TCommand : ICommand
    {
        new Task Handle(TCommand command);

        async Task<None> ICommandHandler<TCommand, None>.Handle(TCommand command)
        {
            await Handle(command);
            return None.Instance;
        }
    }

    /// <summary>
    ///     Very generic handler abstraction used primarily for type restrictions
    ///     in configuration and other internal logic.
    /// </summary>
    public interface IAbstractCommandHandler
    {
    }
}