using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for storage based message handling client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures messaging client to use local single provider.
    /// </summary>
    public static MessagingClientBuilder UseLocalSingleProvider(this MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .UseLocalSingleProvider()
                .AddSingle<IAbstractMessage, CachingResult>() // CachingInterceptor's requirement
                .AddSingle<string, CachingResult>() // GenericMessagingHandlerProxy's requirement
                .AddSinglePartitioned<string, IAbstractMessage>() // GenericMessagingHandlerProxy's requirement
                .AddSingle<string, RemoteHandlerModel>()) // GenericMessagingHandlerProxy's requirement
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseGenericSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Configures the messaging client to use storage based single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured separately in <see cref="StorageBuilder"/>
    ///     or specific provider extension like <see cref="UseLocalSingleProvider"/>.
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
    /// <param name="options"/>
    /// <param name="messageType">Accepting message type.</param>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddGeneric(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.Create<GenericMessagingHandlerProxy>());

        return options;
    }

    /// <summary>
    ///     Registers storage based any message type handling except defined explicitly.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>, including storage configuration.
    /// </remarks>
    public static MessagingClientOptions AddGenericAny(this MessagingClientOptions options)
    {
        options.AnyProvider = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.Create<GenericMessagingHandlerProxy>());

        return options;
    }
}
