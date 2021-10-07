using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Remote message handling middleware.
    /// </summary>
    internal class MessageHandlingMiddleware : IMiddleware
    {
        private readonly IMessagingClient client;

        public MessageHandlingMiddleware(IMessagingClient client) =>
            this.client = client;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != HttpMethods.Post
                || !context.Request.Path.StartsWithSegments("/messages"))
            {
                await next(context);
                return;
            }

            var message = await context.ReadMessageObject();
            var response = await client.SendObject(message);

            await context.WriteMessageResponse(StatusCodes.Status200OK, response);
        }
    }
}
