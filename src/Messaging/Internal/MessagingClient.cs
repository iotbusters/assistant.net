using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Options;
using System;
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
        private readonly IOptionsMonitor<MessagingClientOptions> options;
        private readonly IServiceProvider provider;

        public MessagingClient(
            IOptionsMonitor<MessagingClientOptions> options,
            IServiceProvider provider)
        {
            this.options = options;
            this.provider = provider;
        }

        /// <exception cref="MessageNotRegisteredException"/>
        public Task<object> RequestObject(object message, CancellationToken token)
        {
            var client = CreateInterceptingHandler(message.GetType());
            return client.Request(message, token);
        }

        /// <exception cref="MessageNotRegisteredException"/>
        public Task PublishObject(object message, CancellationToken token)
        {
            var client = CreateInterceptingHandler(message.GetType());
            return client.Publish(message, token);
        }

        /// <exception cref="MessageNotRegisteredException"/>
        private InterceptingMessageHandler CreateInterceptingHandler(Type messageType)
        {
            if(!options.CurrentValue.Handlers.TryGetValue(messageType, out var factory))
                throw new MessageNotRegisteredException(messageType);

            var handler = factory(provider);

            var interceptors = options.CurrentValue.Interceptors
                .Where(x => x.MessageType.IsAssignableFrom(messageType))
                .Reverse()
                .Select(x => x.Factory(provider));

            return new InterceptingMessageHandler(handler, interceptors);
        }
    }
}
