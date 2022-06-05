using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for MongoDB client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures the messaging client to use MongoDB single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    public static MessagingClientOptions UseMongoSingleProvider(this MessagingClientOptions options) => options
        .UseSingleProvider(p => p.Create<MongoMessageHandlerProxy>());

    /// <summary>
    ///     Registers remote MongoDB based handler of <paramref name="messageType"/> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddMongo(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.Create<MongoMessageHandlerProxy>());

        return options;
    }
}
