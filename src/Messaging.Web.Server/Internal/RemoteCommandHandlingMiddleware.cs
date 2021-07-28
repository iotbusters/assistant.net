using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Routingless remote command handling middleware.
    /// </summary>
    internal class RemoteCommandHandlingMiddleware : RemoteCommandHandlingEndpointMiddleware
    {
        private readonly RequestDelegate next;

        public RemoteCommandHandlingMiddleware(RequestDelegate next, ICommandClient client) : base(client) =>
            this.next = next;

        public override Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Method == HttpMethods.Post
                && httpContext.Request.Path.StartsWithSegments("/command"))
                return base.Invoke(httpContext);

            return next(httpContext);
        }
    }
}