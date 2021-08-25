using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Routingless remote message handling middleware.
    /// </summary>
    internal class RemoteMessageHandlingMiddleware : RemoteMessageHandlingEndpointMiddleware
    {
        private readonly RequestDelegate next;

        public RemoteMessageHandlingMiddleware(RequestDelegate next, IMessagingClient client) : base(client) =>
            this.next = next;

        public override Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Method == HttpMethods.Post
                && httpContext.Request.Path.StartsWithSegments("/messages"))
                return base.Invoke(httpContext);

            return next(httpContext);
        }
    }
}