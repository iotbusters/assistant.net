using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Endpoint based remote message handling middleware.
    /// </summary>
    internal class RemoteMessageHandlingEndpointMiddleware
    {
        private readonly IMessagingClient client;

        public RemoteMessageHandlingEndpointMiddleware(IMessagingClient client) =>
            this.client = client;

        public virtual async Task Invoke(HttpContext context)
        {
            var message = await context.ReadMessageObject();
            var response = await client.Send(message);

            await context.WriteMessageResponse(200, response);
        }
    }
}