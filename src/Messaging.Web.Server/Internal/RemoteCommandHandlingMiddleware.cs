using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteCommandHandlingMiddleware : RemoteCommandHandlingEndpointMiddleware
    {
        private readonly RequestDelegate next;

        public RemoteCommandHandlingMiddleware(RequestDelegate next) =>
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