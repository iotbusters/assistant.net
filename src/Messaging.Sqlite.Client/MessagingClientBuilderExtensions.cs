using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     SQLite based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures SQLite single provider dependencies for messaging client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseSqlite{TBuilder}(IBuilder{TBuilder},string)"/> to configure.
    /// </remarks>
    public static MessagingClientBuilder UseSqliteSingleProvider(this MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .UseSqliteSingleProvider()
                .AddSinglePartitioned<int, IAbstractMessage>()
                .AddSingle<int, CachingResult>())
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseSqliteSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Configures SQLite provider dependencies for messaging client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="BuilderExtensions.UseSqlite{TBuilder}(IBuilder{TBuilder},string)"/> to configure;
    ///     It should be added if <see cref="AddSqlite"/> wasn't configured on the start.
    /// </remarks>
    public static MessagingClientBuilder UseSqliteProvider(this MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .UseSqliteSingleProvider()
                .AddSinglePartitioned<int, IAbstractMessage>()
                .AddSingle<int, CachingResult>())
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseSqliteSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Registers remote SQLite based handler of <typeparamref name="TMessage" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSqlite<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddSqlite(typeof(TMessage));

    /// <summary>
    ///     Registers remote SQLite based handler of <paramref name="messageType" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSqlite(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services
            .AddStorage(builder.Name, b => b
                .AddSqlitePartitioned<int, IAbstractMessage>()
                .AddSqlite<int, CachingResult>())
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddSqlite(messageType));
        return builder;
    }
}
