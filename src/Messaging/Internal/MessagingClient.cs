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
        private readonly IServiceProvider provider;

        public MessagingClient(
            IOptions<MessagingClientOptions> options,
            IServiceProvider provider)
        {
            interceptorMap = (
                from interceptorType in options.Value.Interceptors
                from interfaceType in interceptorType.GetMessageInterceptorInterfaceTypes()
                let messageType = interfaceType.GetGenericArguments().First()
                let abstractInterceptorType = typeof(AbstractInterceptor<,,>)
                    .MakeGenericType(interceptorType, messageType, messageType.GetResponseType()!)
                select new KeyValuePair<Type, Type>(messageType, abstractInterceptorType)).ToArray();
            this.provider = provider;
        }

        /// <exception cref="MessageNotRegisteredException"/>
        public Task<object> SendObject(object message, CancellationToken token)
        {
            var handler = CreateInterceptingHandler(message.GetType());
            return handler.Handle(message, token);
        }

        /// <exception cref="MessageNotRegisteredException"/>
        private IAbstractHandler CreateInterceptingHandler(Type messageType)
        {
            var handlerType = typeof(AbstractHandler<,>).MakeGenericTypeBoundToMessage(messageType);
            var handler = provider.GetRequiredService(handlerType);

            var interceptors = interceptorMap
                .Where(x => x.Key.IsAssignableFrom(messageType))
                .Reverse()
                .Select(x => (IAbstractInterceptor)provider.GetRequiredService(x.Value)).ToArray();

            return new AbstractInterceptingHandler((IAbstractHandler)handler, interceptors);
        }
    }
}
