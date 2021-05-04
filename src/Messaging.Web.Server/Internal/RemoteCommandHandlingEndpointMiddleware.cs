using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    internal class RemoteCommandHandlingEndpointMiddleware
    {
        private readonly ICommandClient client;

        public RemoteCommandHandlingEndpointMiddleware(ICommandClient client) =>
            this.client = client;

        public virtual async Task Invoke(HttpContext context)
        {
            dynamic command = await context.ReadCommandObject();

            object response = await client.Send(command);

            await context.WriteCommandResponse(200, response);
        }
    }
}