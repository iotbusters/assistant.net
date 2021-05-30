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
            return CreateInterceptableHandle(commandType, provider, handler.Handle);
        }

        private IAbstractHandler CreateInterceptableHandle(Type commandType, IServiceProvider provider, Func<object, Task<object>> commandHandle)
        {
            var interceptableHandle = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(commandType))
                .Reverse()
                .Select(x =>
                {
                    var adaptorType = typeof(InterceptorAdaptor<,>).MakeGenericTypeBoundToCommand(x.Key);
                    var interceptor = provider.GetRequiredService(x.Value);

                    var adaptor = provider.GetRequiredService(x.Value);
                    ((IInterceptorAdaptorContext)adaptor).Init((dynamic)interceptor);

                    return (adaptor as IAbstractInterceptor)!;
                })
                .Aggregate(commandHandle, (next, current) =>
                    x => current.Intercept(x, next));

            return new DelegatingAbstractHandler(interceptableHandle);
        }

        private static IAbstractHandler CreateHandler(Type commandType, IServiceProvider provider)
        {
            var adaptorType = typeof(HandlerAdapter<,>).MakeGenericTypeBoundToCommand(commandType);
            var handlerType = typeof(ICommandHandler<,>).MakeGenericTypeBoundToCommand(commandType);

            var handler = provider.GetService(handlerType);
            if (handler == null)
                throw new CommandNotRegisteredException(commandType);

            var adaptor = provider.GetRequiredService(adaptorType);
            ((IHandlerAdaptorContext)adaptor).Init((dynamic)handler);

            return (adaptor as IAbstractHandler)!;
        }
    }
}