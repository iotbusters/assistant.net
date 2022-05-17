using Assistant.Net.Diagnostics.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Operation tracking middleware.
/// </summary>
internal class DiagnosticMiddleware : IMiddleware
{
    private readonly IDiagnosticFactory operationFactory;

    public DiagnosticMiddleware(IDiagnosticFactory operationFactory) =>
        this.operationFactory = operationFactory;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Post
            || !context.Request.Path.StartsWithSegments("/messages"))
        {
            await next(context);
            return;
        }

        var messageName = context.GetMessageName().ToLower();
        var operation = operationFactory.Start($"{messageName}-handling-remote-server");

        try
        {
            await next(context);
            operation.Complete();
        }
        catch (Exception)
        {
            operation.Fail();
            throw;
        }
    }
}
