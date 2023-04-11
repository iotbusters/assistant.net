using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for WEB client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures the messaging client to use a single provider feature.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    public static MessagingClientOptions UseWebSingleProvider(this MessagingClientOptions options) => options
        .UseSingleHandler((p, _) => p.Create<WebMessageHandlerProxy>());

    /// <summary>
    ///     Registers remote WEB handler of <paramref name="messageType"/> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    /// <param name="options"/>
    /// <param name="messageType">Accepting message type.</param>
    public static MessagingClientOptions AddWeb(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.HandlerFactories[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.Create<WebMessageHandlerProxy>());

        return options;
    }

    /// <summary>
    ///     Registers remote WEB handler of any message type except explicitly registered.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    public static MessagingClientOptions AddWebAny(this MessagingClientOptions options) => options
        .UseBackoffHandler((p, _) => p.Create<WebMessageHandlerProxy>());
}
