using Assistant.Net.Messaging.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging;

/// <summary>
///     WEB handling server options extensions.
/// </summary>
public static class WebHandlingServerOptionsExtensions
{
    /// <summary>
    ///     Configures the WEB handling server to accept <paramref name="messageType"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="messageType">Accepting message type.</param>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerOptions AcceptMessage(this WebHandlingServerOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));
        options.MessageTypes.Add(messageType);
        return options;
    }

    /// <summary>
    ///     Configures the WEB handling server to accept multiple <paramref name="messageTypes"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="messageTypes">Accepting message types.</param>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerOptions AcceptMessages(this WebHandlingServerOptions options, params Type[] messageTypes)
    {
        foreach (var messageType in messageTypes)
            options.AcceptMessage(messageType);
        return options;
    }

    /// <summary>
    ///     Configures the WEB handling server to accept messages handling by the <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerOptions AcceptMessagesFromHandler(this WebHandlingServerOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        options.AcceptMessages(messageTypes);
        return options;
    }

    /// <summary>
    ///     Configures the generic handling server to accept or not any message types.
    /// </summary>
    public static WebHandlingServerOptions BackoffHandler(this WebHandlingServerOptions options, bool hasHandler)
    {
        options.HasBackoffHandler = hasHandler;
        return options;
    }
}
