using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Adaptor between de-typed interceptors to strongly typed presentation.
    /// </summary>
    internal class InterceptorAdaptor<TCommand, TResponse> : IInterceptorAdaptorContext, IAbstractInterceptor
        where TCommand : ICommand<TResponse>
    {
        private ICommandInterceptor<TCommand, TResponse>? interceptor;

        private ICommandInterceptor<TCommand, TResponse> Interceptor =>
            interceptor ?? throw new InvalidOperationException();

        void IInterceptorAdaptorContext.Init(IAbstractCommandInterceptor interceptor) =>
            this.interceptor = interceptor as ICommandInterceptor<TCommand, TResponse>
                           ?? throw new InvalidOperationException("Unexpected handler type.");

        public Task<object> Intercept(object command, Func<object, Task<object>> next) =>
            Intercept((TCommand)command, async x => (TResponse)await next(x)).Map(x => (object)x!);

        public Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next) =>
            Interceptor.Intercept(command, next);
    }
}