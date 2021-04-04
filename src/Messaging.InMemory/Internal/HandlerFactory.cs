using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;

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
            handlerMap = (from definition in options.Value.Handlers
                          from interfaceType in definition.Type.GetInterfaces()
                          where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                          let commandType = interfaceType.GetGenericArguments().First()
                          select new { commandType, definition.Type }).ToDictionary(x => x.commandType, x => x.Type);

            interceptorMap = (from definition in options.Value.Interceptors
                              from interfaceType in definition.Type.GetInterfaces()
                              where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandInterceptor<,>)
                              let commandType = interfaceType.GetGenericArguments().First()
                              select new KeyValuePair<Type, Type>(commandType, definition.Type)).ToArray();
            this.scopeFactory = scopeFactory;
        }

        public IAbstractHandler Create(Type commandType)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var handler = CreateHandler(commandType, provider)
                          ?? throw new CommandNotRegisteredException(commandType);

            return CreateInterceptableHandle(commandType, provider, handler.Handle);
        }

        private IAbstractHandler? CreateHandler(Type commandType, IServiceProvider provider)
        {
            var adapterType = typeof(HandlerAdaptor<,>).MakeGenericTypeBoundToCommand(commandType);

            if (!handlerMap.TryGetValue(commandType, out var handlerType))
                return null;

            var commandHandler = ActivatorUtilities.CreateInstance(provider, handlerType);
            return (IAbstractHandler)ActivatorUtilities.CreateInstance(provider, adapterType, commandHandler);
        }

        private IAbstractHandler CreateInterceptableHandle(Type commandType, IServiceProvider provider, Func<object, Task<object>> commandHandle)
        {
            var interceptableHandle = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Reverse()
                .Select(x =>
                {
                    var adaptorType = typeof(InterceptorAdaptor<,>).MakeGenericTypeBoundToCommand(x.Key);
                    var interceptor = ActivatorUtilities.CreateInstance(provider, x.Value);
                    return (IAbstractInterceptor)ActivatorUtilities.CreateInstance(provider, adaptorType, interceptor);
                })
                .Aggregate(commandHandle, (next, current) =>
                    x => current.Intercept(x, next));

            return new DelegatingAbstractHandler(interceptableHandle);
        }
    }
}