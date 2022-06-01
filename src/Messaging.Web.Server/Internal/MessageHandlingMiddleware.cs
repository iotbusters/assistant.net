using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Remote message handling middleware.
/// </summary>
internal class MessageHandlingMiddleware : IMiddleware
{
    private readonly IOptionsMonitor<WebHandlingServerOptions> options;

    public MessageHandlingMiddleware(IOptionsMonitor<WebHandlingServerOptions> options)
    {
        this.options = options;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Post
            || !context.Request.Path.StartsWithSegments("/messages"))
        {
            await next(context);
            return;
        }

        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        provider.ConfigureNamedOptionContext(WebOptionsNames.DefaultName);

        var client = provider.GetRequiredService<IMessagingClient>();

        var serverOptions = options.CurrentValue;

        var message = await context.ReadMessageObject();
        if (!serverOptions.MessageTypes.Contains(message.GetType()))
            throw new MessageNotRegisteredException(message.GetType());

        var response = await client.RequestObject(message);

        await context.WriteMessageResponse(StatusCodes.Status200OK, response);
    }
}
