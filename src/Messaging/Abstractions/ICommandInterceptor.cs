using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command interceptor abstraction that accepts <typeparamref name="TCommand" /> and its children.
    ///     It's one piece in an intercepting chain with control over command and response.
    /// </summary>
    public interface ICommandInterceptor<TCommand, TResponse> : IAbstractInterceptor
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        ///     Intercepts the <paramref name="command" /> or one of its children
        ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
        /// </summary>
        Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next);

        Task<object> IAbstractInterceptor.Intercept(object command, Func<object, Task<object>> next) =>
            Intercept((TCommand) command, x => next(x).MapSuccess(y => (TResponse) y)).MapSuccess(y => (object) y!);
    }

    /// <summary>
    ///     Command interceptor abstraction that accepts <typeparamref name="TCommand" /> and its children.
    ///     It's one piece in an intercepting chain with control over command with no response expectation.
    /// </summary>
    public interface ICommandInterceptor<TCommand> : ICommandInterceptor<TCommand, None>
        where TCommand : ICommand<None>
    {
        /// <summary>
        ///     Intercepts the <paramref name="command" /> or one of its children
        ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
        /// </summary>
        Task Intercept(TCommand command, Func<TCommand, Task> next);

        async Task<None> ICommandInterceptor<TCommand, None>.Intercept(TCommand command, Func<TCommand, Task<None>> next)
        {
            await Intercept(command, next);
            return None.Instance;
        }
    }

    /// <summary>
    ///     Simple alias to interceptor that can handle all types of commands.
    /// </summary>
    public interface ICommandInterceptor : ICommandInterceptor<ICommand<object>, object>
    {
    }
}