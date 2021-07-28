using Assistant.Net.Messaging.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Endpoint based remote command handling middleware.
    /// </summary>
    internal class RemoteCommandHandlingEndpointMiddleware
    {
        private readonly ICommandClient client;

        public RemoteCommandHandlingEndpointMiddleware(ICommandClient client) =>
            this.client = client;

        public virtual async Task Invoke(HttpContext context)
        {
            var command = await context.ReadCommandObject();
            var response = await client.Send(command);

            await context.WriteCommandResponse(200, response);
        }
    }
}