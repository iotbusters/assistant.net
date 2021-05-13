using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command interceptor abstraction that accepts <typeparamref name="TCommand" /> and its children.
    ///     It's one piece in an intercepting chain with control over command and response.
    /// </summary>
    public interface ICommandInterceptor<TCommand, TResponse> : IAbstractCommandInterceptor
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        ///     Intercepts the <paramref name="command" /> or one of its children
        ///     and delegates the call to the <paramref name="next" /> interceptor if needed.
        /// </summary>
        Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next);
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

    /// <summary>
    ///     Very generic interceptor abstraction used primarily for type restrictions
    ///     in configuration and other internal logic.
    /// </summary>
    public interface IAbstractCommandInterceptor
    {
    }
}