using Assistant.Net.Analyzers;
using Assistant.Net.Analyzers.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Configuration based default de-typed command handler implementation.
    /// </summary>
    internal class HandlerFactory : IHandlerFactory
    {
        private readonly IEnumerable<KeyValuePair<Type, Type>> interceptorMap;
        private readonly IProxyFactory proxyFactory;
        private readonly IServiceScopeFactory scopeFactory;

        public HandlerFactory(
            IOptions<CommandClientOptions> options,
            IProxyFactory proxyFactory,
            IServiceScopeFactory scopeFactory)
        {
            interceptorMap = (from type in options.Value.Interceptors
                              from interfaceType in type.GetInterfaces()
                              where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandInterceptor<,>)
                              let commandType = interfaceType.GetGenericArguments().First()
                              select new KeyValuePair<Type, Type>(commandType, type)).ToArray();
            this.proxyFactory = proxyFactory;
            this.scopeFactory = scopeFactory;
        }

        public IAbstractHandler Create(Type commandType)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            return CreateInterceptingHandler(commandType, provider);
        }

        private IAbstractHandler CreateInterceptingHandler(Type commandType, IServiceProvider provider)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericTypeBoundToCommand(commandType);

            var handler = provider.GetService(handlerType);
            if (handler == null)
                throw new CommandNotRegisteredException(commandType);

            var proxy = proxyFactory.Create((IAbstractHandler) handler);

            var interceptors = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Reverse()
                .Select(x => (IAbstractInterceptor) provider.GetRequiredService(x.Value));

            foreach (var interceptor in interceptors)
                proxy.Intercept(
                    x => x.Handle(default!),
                    (next, args) => interceptor.Intercept(args[0]!, x => next(new[] {x})));

            return proxy.Object;
        }
    }
}