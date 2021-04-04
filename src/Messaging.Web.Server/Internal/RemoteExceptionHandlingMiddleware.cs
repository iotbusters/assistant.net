using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Assistant.Net.Messaging.Internal
{
    internal class RemoteExceptionHandlingMiddleware
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
            catch (CommandException ex)
            {
                await context.WriteCommandResponse(500, ex);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await context.WriteCommandResponse(500, new CommandFailedException(ex));
            }
        }
    }
}