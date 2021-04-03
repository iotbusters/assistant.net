using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteCommandHandlingMiddleware : RemoteCommandHandlingEndpointMiddleware
    {
        private readonly RequestDelegate next;

        public RemoteCommandHandlingMiddleware(
            RequestDelegate next,
            ISystemLifetime lifetime,
            IOptions<JsonSerializerOptions> serializerOptions,
            IOptionsMonitor<CommandOptions> commandOptions,
            IServiceScopeFactory scopeFactory)
            : base(lifetime, serializerOptions, commandOptions, scopeFactory) =>
            this.next = next;

        public override Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("command"))
                return base.Invoke(httpContext);

            return next(httpContext);
        }
    }
}