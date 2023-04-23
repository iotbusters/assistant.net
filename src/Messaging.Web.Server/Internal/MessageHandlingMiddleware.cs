using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     WEB message handling middleware.
/// </summary>
internal class MessageHandlingMiddleware : IMiddleware, IDisposable
{
    private readonly Random random = new();
    private readonly Dictionary<string, WebHandlingServerOptions> serverOptions = new();
    private readonly Dictionary<Type, List<string>> messageNameToOptionMapping = new();
    private readonly IDisposable disposable;

    public MessageHandlingMiddleware(string[] names, IOptionsMonitor<WebHandlingServerOptions> options)
    {
        if (names.Length == 0)
            names = new[] { Microsoft.Extensions.Options.Options.DefaultName };

        disposable = options.OnChange((newOptions, name) =>
        {
            if (name == null || !names.Contains(name))
                return;

            var previousMessageTypes = this.serverOptions.TryGetValue(name, out var previousOptions)
                ? previousOptions.MessageTypes
                : new(0);

            this.serverOptions[name] = newOptions;

            var unregisterMessageTypes = previousMessageTypes.Except(newOptions.MessageTypes);
            foreach (var messageType in unregisterMessageTypes)
                if (messageNameToOptionMapping.TryGetValue(messageType, out var list))
                {
                    list.Remove(name);
                    if (list.Count == 0)
                        messageNameToOptionMapping.Remove(messageType);
                }

            var registerMessageTypes = newOptions.MessageTypes.Except(previousMessageTypes);
            foreach (var messageType in registerMessageTypes)
                if (!messageNameToOptionMapping.TryGetValue(messageType, out var _))
                    messageNameToOptionMapping.Add(messageType, new() {name});
        })!;

        var pairs =
            from name in names
            let so = options.Get(name)
            from messageType in so.MessageTypes
            group name by messageType into pair
            select pair;
        foreach (var pair in pairs)
            messageNameToOptionMapping[pair.Key] = pair.ToList();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Post
            || !context.Request.Path.StartsWithSegments("/messages"))
        {
            await next(context);
            return;
        }

        var message = await context.ReadMessageObject();
        if (!messageNameToOptionMapping.TryGetValue(message.GetType(), out var names))
            throw new MessageNotRegisteredException(message.GetType());

        var name = PickOneOf(names);
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        provider.ConfigureNamedOptionContext(name);

        var client = provider.GetRequiredService<IMessagingClient>();
        var response = await client.RequestObject((IAbstractMessage)message);

        await context.WriteMessageResponse(StatusCodes.Status200OK, response);
    }

    private string PickOneOf(IReadOnlyList<string> names)
    {
        if (names.Count == 1)
            return names[0];

        var roundRobinIndex = random.Next(0, names.Count - 1);
        var name = names[roundRobinIndex];
        return name;
    }

    public void Dispose() => disposable.Dispose();
}
