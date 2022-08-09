using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     WEB oriented messaging client configuration extensions.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to use a WEB single provider implementation.
    /// </summary>
    public static MessagingClientBuilder UseWebSingleProvider(this MessagingClientBuilder builder, Action<IHttpClientBuilder> configureBuilder)
    {
        builder.Services
            .ConfigureJsonSerialization(builder.Name)
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseWebSingleProvider());
        return builder.UseWeb(configureBuilder);
    }

    /// <summary>
    ///     Configures the messaging client to connect the remote web handler.
    /// </summary>
    public static MessagingClientBuilder UseWeb(this MessagingClientBuilder builder, Action<IHttpClientBuilder> configureBuilder)
    {
        var clientBuilder = builder.Services
            .ConfigureJsonSerialization(builder.Name)
            .AddRemoteWebMessagingClient();
        configureBuilder.Invoke(clientBuilder);
        return builder;
    }

    /// <summary>
    ///     Registers remote WEB handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling <see cref="UseWeb"/>.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddWeb<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddWeb(typeof(TMessage));

    /// <summary>
    ///     Registers remote WEB handler of <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling <see cref="UseWeb"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    /// <param name="builder"/>
    /// <param name="messageType">The message type to find associated handler.</param>
    public static MessagingClientBuilder AddWeb(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException("Invalid message type.", nameof(messageType));

        builder.Services
            .ConfigureJsonSerialization(builder.Name)
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddWeb(messageType));
        return builder;
    }

    /// <summary>
    ///     Registers remote WEB handler of any message type except explicitly registered.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling <see cref="UseWeb"/>.
    /// </remarks>
    public static MessagingClientBuilder AddWebAny(this MessagingClientBuilder builder)
    {
        builder.Services
            .ConfigureJsonSerialization(builder.Name)
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddWebAny());
        return builder;
    }
}
