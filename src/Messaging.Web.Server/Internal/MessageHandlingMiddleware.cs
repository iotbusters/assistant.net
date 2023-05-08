using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     WEB message handling middleware.
/// </summary>
internal class MessageHandlingMiddleware : IMiddleware
{
    private readonly string name;
    private readonly WebHandlingServerOptions options;

    /// <summary>
    ///     Creates a backoff instance to fail the request if not handled yet.
    /// </summary>
    public MessageHandlingMiddleware()
    {
        name = null!;
        options = null!;
    }

    /// <summary>
    ///     Creates a named instance.
    /// </summary>
    public MessageHandlingMiddleware(string name, IOptionsMonitor<WebHandlingServerOptions> options)
    {
        this.name = name;
        this.options = options.Get(name);
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Post
            || !context.Request.Path.StartsWithSegments("/messages"))
        {
            await next(context);
            return;
        }

        var messageType = context.GetMessageType();

        if (options == null!)
            throw new MessageNotRegisteredException(messageType);

        if (!options.HasBackoffHandler && !options.MessageTypes.Contains(messageType))
        {
            await next(context);
            return;
        }

        context.RequestServices.ConfigureNamedOptionContext(name);

        var client = context.RequestServices.GetRequiredService<IMessagingClient>();
        var message = await context.ReadMessageObject();
        var response = await client.RequestObject((IAbstractMessage)message);

        await context.WriteMessageResponse(StatusCodes.Status200OK, response);
    }
}
