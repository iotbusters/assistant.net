using Assistant.Net.Messaging.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Global error handling middleware.
    /// </summary>
    internal class RemoteExceptionHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != HttpMethods.Post
                || !context.Request.Path.StartsWithSegments("/messages"))
            {
                await next(context);
                return;
            }

            try
            {
                await next(context);
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

            if (ex is MessageDeferredException
                || ex is TimeoutException
                || ex is OperationCanceledException)
                return context.WriteMessageResponse(202);

            if (ex is MessageNotFoundException
                || ex is MessageNotRegisteredException)
                return context.WriteMessageResponse(404, ex);

            if (ex is MessageContractException)
                return context.WriteMessageResponse(400, ex);

            if (ex is MessageException)
                return context.WriteMessageResponse(500, ex);

            return context.WriteMessageResponse(500, new MessageFailedException(ex));
        }
    }
}
