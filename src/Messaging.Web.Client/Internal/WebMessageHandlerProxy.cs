using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed proxy to remote message handling.
    /// </summary>
    internal class WebMessageHandlerProxy<TMessage, TResponse> : IMessageHandler<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly ILogger logger;
        private readonly IWebMessageHandlerClient client;

        public WebMessageHandlerProxy(
            ILogger<WebMessageHandlerProxy<TMessage, TResponse>> logger,
            IWebMessageHandlerClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<TResponse> Handle(TMessage message, CancellationToken token)
        {
            var messageName = message.GetType().Name;

            logger.LogInformation("{Message} handling has been requested.", messageName);

            try
            {
                var response = await client.DelegateHandling(message, token);
                logger.LogInformation("{Message} handling request succeeded.", messageName);
                return response;
            }
            catch (MessageDeferredException ex)
            {
                logger.LogInformation(ex, "{Message} handing has been deferred.", messageName);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "{Message} handing request has been cancelled or exceeded timeout.", messageName);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Message} handling request has failed.", messageName);
                throw;
            }
        }
    }
}
