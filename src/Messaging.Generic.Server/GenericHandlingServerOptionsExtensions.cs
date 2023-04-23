using Assistant.Net.Messaging.Options;
using System;
using System.Linq;

namespace Assistant.Net.Messaging;

/// <summary>
///     Generic handling server options extensions.
/// </summary>
public static class GenericHandlingServerOptionsExtensions
{
    /// <summary>
    ///     Configures the generic handling server to accept <paramref name="messageType"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="messageType">Accepting message type.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerOptions AcceptMessage(this GenericHandlingServerOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));
        options.MessageTypes.Add(messageType);
        return options;
    }

    /// <summary>
    ///     Configures the generic handling server to accept multiple <paramref name="messageTypes"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="messageTypes">Accepting message types.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerOptions AcceptMessages(this GenericHandlingServerOptions options, params Type[] messageTypes)
    {
        foreach (var messageType in messageTypes)
            options.AcceptMessage(messageType);
        return options;
    }

    /// <summary>
    ///     Configures the generic handling server to accept messages handling by the <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerOptions AcceptMessagesFromHandler(this GenericHandlingServerOptions options, Type handlerType)
    {
        var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First()).ToArray();
        options.AcceptMessages(messageTypes);
        return options;
    }
}
