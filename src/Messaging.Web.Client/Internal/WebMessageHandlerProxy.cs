using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed proxy to remote message handling.
    /// </summary>
    internal class WebMessageHandlerProxy<TMessage, TResponse> : IAbstractHandler
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

        public async Task<object> Request(object message, CancellationToken token = default)
        {
            var messageName = message.GetType().Name;
            var messageId = message.GetSha1();

            logger.LogInformation("Message({MessageName}/{MessageId}): requested.", messageName, messageId);

            try
            {
                var response = await client.DelegateHandling((TMessage)message, token);
                logger.LogInformation("Message({MessageName}/{MessageId}): responded.", messageName, messageId);
                return response!;
            }
            catch (MessageDeferredException ex)
            {
                logger.LogInformation(ex, "Message({MessageName}/{MessageId}): deferred.", messageName, messageId);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}): cancelled or exceeded timeout.", messageName, messageId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}): failed.", messageName, messageId);
                throw;
            }
        }

        public async Task Publish(object message, CancellationToken token)
        {
            var messageName = message.GetType().Name;
            var messageId = message.GetSha1();

            logger.LogInformation("Message({MessageName}/{MessageId}): publishing.", messageName, messageId);

            try
            {
                // note: it gives a 100ms window to fail the request.
                await await Task.WhenAny(
                    client.DelegateHandling((TMessage)message, token),
                    Task.Delay(TimeSpan.FromSeconds(0.1), token));

                logger.LogInformation("Message({MessageName}/{MessageId}): published.", messageName, messageId);
            }
            catch (MessageDeferredException ex)
            {
                logger.LogInformation(ex, "Message({MessageName}/{MessageId}): ignore deferred.", messageName, messageId);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Message({MessageName}/{MessageId}): cancelled or exceeded timeout.", messageName, messageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}): failed.", messageName, messageId);
                throw;
            }
        }
    }
}
