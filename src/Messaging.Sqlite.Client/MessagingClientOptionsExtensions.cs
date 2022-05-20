using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for SQLite client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Registers remote SQLite based handler of <paramref name="messageType" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered handlers.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientOptions AddSqlite(this MessagingClientOptions options, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        options.Handlers[messageType] = new HandlerDefinition(p =>
        {
            var providerType = typeof(SqliteMessageHandlerProxy<,>).MakeGenericTypeBoundToMessage(messageType);
            return (IAbstractHandler)p.GetRequiredService(providerType);
        });

        return options;
    }
}
