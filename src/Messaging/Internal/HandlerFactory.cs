using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Configuration based default de-typed command handler implementation.
    /// </summary>
    internal class HandlerFactory : IHandlerFactory
    {
        private readonly IEnumerable<KeyValuePair<Type, Type>> interceptorMap;
        private readonly IServiceScopeFactory scopeFactory;

        public HandlerFactory(
            IOptions<CommandClientOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            interceptorMap = (from type in options.Value.Interceptors
                              from interfaceType in type.GetInterfaces()
                              where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandInterceptor<,>)
                              let commandType = interfaceType.GetGenericArguments().First()
                              select new KeyValuePair<Type, Type>(commandType, type)).ToArray();
            this.scopeFactory = scopeFactory;
        }

        public IAbstractHandler Create(Type commandType)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var handler = CreateHandler(commandType, provider);
            return CreateInterceptingHandler(commandType, provider, handler.Handle);
        }

        private IAbstractHandler CreateInterceptingHandler(Type commandType, IServiceProvider provider, Func<object, Task<object>> commandHandle)
        {
            var interceptingHandle = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Reverse()
                .Select(x =>
                {
                    var adapter = provider.GetRequiredService(x.Value);
                    var interceptor = (IAbstractCommandInterceptor) provider.GetRequiredService(x.Value);
                    ((IInterceptorAdapterContext) adapter).Init(interceptor);

                    return (IAbstractInterceptor) adapter;
                })
                .Aggregate(commandHandle, (next, current) =>
                    x => current.Intercept(x, next));

            return new DelegatingAbstractHandler(interceptingHandle);
        }

        private static IAbstractHandler CreateHandler(Type commandType, IServiceProvider provider)
        {
            var adapterType = typeof(HandlerAdapter<,>).MakeGenericTypeBoundToCommand(commandType);
            var handlerType = typeof(ICommandHandler<,>).MakeGenericTypeBoundToCommand(commandType);

            var handler = provider.GetService(handlerType);
            if (handler == null)
                throw new CommandNotRegisteredException(commandType);

            var adapter = provider.GetRequiredService(adapterType);
            ((IHandlerAdapterContext) adapter).Init((dynamic) handler);

            return (IAbstractHandler) adapter;
        }
    }
}