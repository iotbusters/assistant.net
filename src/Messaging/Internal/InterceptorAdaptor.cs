using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class InterceptorAdaptor<TCommand, TResponse> : IAbstractInterceptor
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandInterceptor<TCommand, TResponse> interceptor;

        public InterceptorAdaptor(ICommandInterceptor<TCommand, TResponse> interceptor) =>
            this.interceptor = interceptor;

        public async Task<object> Intercept(object command, Func<object, Task<object>> next) =>
            await Intercept((TCommand)command, async x => (TResponse)await next(x));

        public Task<TResponse> Intercept(TCommand command, Func<TCommand, Task<TResponse>> next) =>
            interceptor.Intercept(command, next);
    }
}