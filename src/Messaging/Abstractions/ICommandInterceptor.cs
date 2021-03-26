using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandInterceptor<TCommand, TResponse> : IInterceptor
        where TCommand : ICommand<TResponse>
    {
        Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next);
    }

    public interface ICommandInterceptor<TCommand> : ICommandInterceptor<TCommand, None>
        where TCommand : ICommand<None>
    {
        Task Intercept(TCommand command, Func<TCommand, Task> next);

        async Task<None> ICommandInterceptor<TCommand, None>.Intercept(TCommand command, Func<TCommand, Task<None>> next)
        {
            await Intercept(command, next);
            return None.Instance;
        }
    }

    public interface ICommandInterceptor : ICommandInterceptor<ICommand<object>, object>
    {
    }

    public interface IInterceptor
    {
    }
}