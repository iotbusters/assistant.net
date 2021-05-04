using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Assistant.Net.Messaging.Exceptions;

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
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private Task HandleException(HttpContext context, Exception ex)
        {
            if (ex is AggregateException e)
                return HandleException(context, e.InnerException!);

            if (ex is CommandNotFoundException
                || ex is CommandNotRegisteredException)
                return context.WriteCommandResponse(404, ex);

            if (ex is CommandContractException)
                return context.WriteCommandResponse(400, ex);

            if (ex is CommandException)
                return context.WriteCommandResponse(500, ex);

            return context.WriteCommandResponse(500, new CommandFailedException(ex));
        }
    }
}