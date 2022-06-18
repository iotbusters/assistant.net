﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for storage based message handling client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures the messaging client to use storage based single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured separately in <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    public static MessagingClientOptions UseGenericSingleProvider(this MessagingClientOptions options) => options
        .UseSingleProvider(p => p.Create<GenericMessagingHandlerProxy>());

    /// <summary>
    ///     Registers storage based <paramref name="messageType"/> handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>, including storage configuration.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddGeneric(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.Create<GenericMessagingHandlerProxy>());

        return options;
    }
}