using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandHandler<TCommand, TResponse> : IAbstractCommandHandler where TCommand : ICommand<TResponse>
    {
        Task<TResponse> Handle(TCommand command);
    }

    public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, None> where TCommand : ICommand
    {
        new Task Handle(TCommand command);

        async Task<None> ICommandHandler<TCommand, None>.Handle(TCommand command)
        {
            await Handle(command);
            return None.Instance;
        }
    }

    public interface IAbstractCommandHandler
    {
    }
}