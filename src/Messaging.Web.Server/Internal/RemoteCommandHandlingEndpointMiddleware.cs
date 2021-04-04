using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteCommandHandlingEndpointMiddleware
    {
        public virtual async Task Invoke(HttpContext context)
        {
            dynamic command = await context.ReadCommandObject();

            object response = await context.GetClient().Send(command);

            await context.WriteCommandResponse(200, response);
        }
    }
}