using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Adaptor between de-typed interceptors to strongly typed presentation.
    /// </summary>
    internal class InterceptorAdapter<TCommand, TResponse> : IInterceptorAdapterContext, IAbstractInterceptor
        where TCommand : ICommand<TResponse>
    {
        private ICommandInterceptor<TCommand, TResponse>? interceptor;

        private ICommandInterceptor<TCommand, TResponse> Interceptor =>
            interceptor ?? throw new InvalidOperationException();

        void IInterceptorAdapterContext.Init(IAbstractCommandInterceptor interceptor) =>
            this.interceptor = interceptor as ICommandInterceptor<TCommand, TResponse>
                           ?? throw new InvalidOperationException("Unexpected handler type.");

        public Task<object> Intercept(object command, Func<object, Task<object>> next) =>
            Intercept((TCommand)command, async x => (TResponse)await next(x)).MapSuccess(x => (object)x!);

        public Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next) =>
            Interceptor.Intercept(command, next);
    }
}