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
        private readonly IOptionsMonitor<WebHandlingServerOptions> options;
        private readonly IMessagingClientFactory clientFactory;

        public MessageHandlingMiddleware(
            IOptionsMonitor<WebHandlingServerOptions> options,
            IMessagingClientFactory clientFactory)
        {
            this.options = options;
            this.clientFactory = clientFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != HttpMethods.Post
                || !context.Request.Path.StartsWithSegments("/messages"))
            {
                await next(context);
                return;
            }

            var serverOptions = options.CurrentValue;

            var message = await context.ReadMessageObject();
            if (!serverOptions.MessageTypes.Contains(message.GetType()))
                throw new MessageNotRegisteredException(message.GetType());

            var client = clientFactory.Create(WebOptionsNames.DefaultName);
            var response = await client.RequestObject(message);

            await context.WriteMessageResponse(StatusCodes.Status200OK, response);
        }
    }
}
