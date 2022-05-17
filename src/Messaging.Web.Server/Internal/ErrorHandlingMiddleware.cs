using Assistant.Net.Messaging.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Global error handling middleware.
/// </summary>
internal class ExceptionHandlingMiddleware : IMiddleware
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
            return context.WriteMessageResponse(StatusCodes.Status202Accepted);

        if (ex is MessageNotFoundException
            || ex is MessageNotRegisteredException)
            return context.WriteMessageResponse(StatusCodes.Status404NotFound, ex);

        if (ex is MessageContractException)
            return context.WriteMessageResponse(StatusCodes.Status400BadRequest, ex);

        if (ex is MessageException)
            return context.WriteMessageResponse(StatusCodes.Status500InternalServerError, ex);

        return context.WriteMessageResponse(StatusCodes.Status500InternalServerError, new MessageFailedException(ex));
    }
}
