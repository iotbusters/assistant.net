using System;
using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public RemoteExceptionHandlingMiddleware(RequestDelegate next) =>
            this.next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (CommandNotFoundException ex)
            {
                await context.WriteCommandResponse(404, ex);
            }
            catch (CommandNotRegisteredException ex)
            {
                await context.WriteCommandResponse(404, ex);
            }
            catch (CommandContractException ex)
            {
                await context.WriteCommandResponse(400, ex);
            }
            catch (JsonException ex)
            {
                await context.WriteCommandResponse(400, new CommandContractException("Unexpected content", ex));
            }
            catch (Exception ex)
            {
                await context.WriteCommandResponse(500, ex);
            }
        }
    }
}