using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Routingless remote message handling middleware.
    /// </summary>
    internal class RemoteMessageHandlingMiddleware : IMiddleware
    {
        private readonly IMessagingClient client;

        public RemoteMessageHandlingMiddleware(IMessagingClient client)
        {
            this.client = client;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != HttpMethods.Post
                || !context.Request.Path.StartsWithSegments("/messages"))
            {
                await next(context);
                return;
            }

            var message = await context.ReadMessageObject();
            var response = await client.Send(message);

            await context.WriteMessageResponse(200, response);
        }
    }
}
