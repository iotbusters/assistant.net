using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Internal
{
    internal class HandlerFactory : IHandlerFactory
    {
        private readonly IDictionary<Type, Type> handlerMap;
        private readonly IEnumerable<KeyValuePair<Type, Type>> interceptorMap;
        private readonly IServiceScopeFactory scopeFactory;

        public HandlerFactory(
            IOptions<CommandOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            handlerMap = (from handlerType in options.Value.Handlers
                          from interfaceType in handlerType.GetInterfaces()
                          where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                          let commandType = interfaceType.GetGenericArguments().First()
                          select new { commandType, handlerType }).ToDictionary(x => x.commandType, x => x.handlerType);

            interceptorMap = (from interceptorType in options.Value.Interceptors
                              from interfaceType in interceptorType.GetInterfaces()
                              where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandInterceptor<,>)
                              let commandType = interfaceType.GetGenericArguments().First()
                              select new KeyValuePair<Type, Type>(commandType, interceptorType)).ToArray();
            this.scopeFactory = scopeFactory;
        }

        public IAbstractHandler Create(Type commandType)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var handler = CreateHandler(commandType, provider);

            return CreateInterceptableHandle(commandType, provider, handler.Handle);
        }

        private IAbstractHandler CreateHandler(Type commandType, IServiceProvider provider)
        {
            var responseType = GetResponseType(commandType);
            var adapterType = typeof(HandlerAdapter<,>).MakeGenericType(commandType, responseType);

            if (!handlerMap.TryGetValue(commandType, out var handlerType))
                throw new InvalidOperationException($"No handler registered for {commandType}");

            var commandHandler = provider.GetRequiredService(handlerType);

            return (IAbstractHandler)ActivatorUtilities.CreateInstance(provider, adapterType, commandHandler);
        }

        private IAbstractHandler CreateInterceptableHandle(Type commandType, IServiceProvider provider, Func<object, Task<object>> commandHandle)
        {
            var interceptableHandle = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Reverse()
                .Select(x => (IAbstractInterceptor)ActivatorUtilities.CreateInstance(
                    provider,
                    typeof(InterceptorAdaptor<,>).MakeGenericType(x.Key, GetResponseType(x.Key)),
                    provider.GetRequiredService(x.Value)))
                .Aggregate(
                    commandHandle,
                    (next, current) =>
                        x => current.Intercept(x, next));

            return new DelegatingAbstractHandler(interceptableHandle);
        }

        private static Type GetResponseType(Type commandType) => commandType
            .GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>))
            .GetGenericArguments()
            .Single();

        internal interface IAbstractInterceptor
        {
            Task<object> Intercept(object command, Func<object, Task<object>> next);
        }

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
}