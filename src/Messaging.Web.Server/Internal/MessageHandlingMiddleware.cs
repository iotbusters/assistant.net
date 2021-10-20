using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Remote message handling middleware.
    /// </summary>
    internal class MessageHandlingMiddleware : IMiddleware
    {
        private readonly IOptions<WebHandlingServerOptions> options;
        private readonly IMessagingClient client;

        public MessageHandlingMiddleware(
            IOptions<WebHandlingServerOptions> options,
            IMessagingClient client)
        {
            this.options = options;
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
            if (!options.Value.MessageTypes.Contains(message.GetType()))
                throw new MessageNotRegisteredException(message.GetType());

            var response = await client.SendObject(message);

            await context.WriteMessageResponse(StatusCodes.Status200OK, response);
        }
    }
}
