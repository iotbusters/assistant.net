using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures MongoDB single provider dependencies for messaging client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseMongo{TBuilder}(IBuilder{TBuilder},string)"/> to configure.
    /// </remarks>
    public static MessagingClientBuilder UseMongoSingleProvider(this MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .UseMongoSingleProvider()
                .AddSinglePartitioned<int, IAbstractMessage>()
                .AddSingle<int, CachingResult>())
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseMongoSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Configures MongoDB provider dependencies for messaging client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseMongo{TBuilder}(IBuilder{TBuilder},string)"/> to configure;
    ///     It should be added if <see cref="AddMongo"/> wasn't configured on the start.
    /// </remarks>
    public static MessagingClientBuilder UseMongoProvider(this MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .AddMongoPartitioned<int, IAbstractMessage>()
                .AddMongo<int, CachingResult>());
        return builder;
    }

    /// <summary>
    ///     Registers remote MongoDB based handler of <typeparamref name="TMessage"/> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

    /// <summary>
    ///     Registers remote MongoDB based handler of <paramref name="messageType"/> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddMongo(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddMongo(messageType));
        return builder.UseMongoProvider();
    }
}
