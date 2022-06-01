using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for SQLite client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures the messaging client to use SQLite single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    public static MessagingClientOptions UseSqliteSingleProvider(this MessagingClientOptions options) => options
        .UseSingleProvider(p => p.GetRequiredService<SqliteMessageHandlerProxy>());

    /// <summary>
    ///     Registers remote SQLite based handler of <paramref name="messageType" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers;
    ///     it has dependencies configured by <see cref="MessagingClientBuilder"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddSqlite(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new InstanceCachingFactory<IAbstractHandler>(p =>
            p.GetRequiredService<SqliteMessageHandlerProxy>());

        return options;
    }
}
