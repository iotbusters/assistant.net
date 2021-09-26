using Assistant.Net.Dynamics;
using Assistant.Net.Dynamics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Default messaging client implementation.
    /// </summary>
    internal class MessagingClient : IMessagingClient
    {
        private readonly IEnumerable<KeyValuePair<Type, Type>> interceptorMap;
        private readonly IProxyFactory proxyFactory;
        private readonly IServiceScopeFactory scopeFactory;

        public MessagingClient(
            IOptions<MessagingClientOptions> options,
            IProxyFactory proxyFactory,
            IServiceScopeFactory scopeFactory)
        {
            interceptorMap = (from type in options.Value.Interceptors
                              from interfaceType in type.GetInterfaces()
                              where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IMessageInterceptor<,>)
                              let messageType = interfaceType.GetGenericArguments().First()
                              select new KeyValuePair<Type, Type>(messageType, type)).ToArray();
            this.proxyFactory = proxyFactory;
            this.scopeFactory = scopeFactory;
        }

        /// <exception cref="MessageNotRegisteredException"/>
        public Task<object> SendObject(object message, CancellationToken token)
        {
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var handler = CreateInterceptingHandler(message.GetType(), provider);
            return handler.Handle(message, token);
        }

        /// <exception cref="MessageNotRegisteredException"/>
        private IAbstractHandler CreateInterceptingHandler(Type messageType, IServiceProvider provider)
        {
            var handlerType = typeof(IMessageHandler<,>).MakeGenericTypeBoundToMessage(messageType);

            var handler = provider.GetService(handlerType);
            if (handler == null)
                throw new MessageNotRegisteredException(messageType);

            var proxy = proxyFactory.Create((IAbstractHandler) handler);

            var interceptors = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(messageType))
                .Reverse()
                .Select(x => (IAbstractInterceptor)provider.GetRequiredService(x.Value));

            foreach (var interceptor in interceptors)
                proxy.Intercept(
                    selector: x => x.Handle(default!, default!),
                    interceptor: (next, args) =>
                    {
                        var message = args[0]!;
                        var token = (CancellationToken)args[1]!;
                        token.ThrowIfCancellationRequested();

                        return interceptor.Intercept((m, t) => next(new[] {m, t}), message, token);
                    });

            return proxy.Object;
        }
    }
}
