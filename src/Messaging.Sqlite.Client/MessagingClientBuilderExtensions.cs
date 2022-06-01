using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     SQLite based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to use a SQLite single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseSqlite(MessagingClientBuilder,string)"/> to configure.
    /// </remarks>
    public static MessagingClientBuilder UseSqliteSingleProvider(this MessagingClientBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseSqliteSingleProvider())
            .ConfigureStorage(builder.Name, b => b.UseSqliteSingleProvider());
        return builder.AddSqliteSingleProvider(b => b.UseSqlite(configureOptions));
    }

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, string connectionString) =>
        builder.UseSqlite(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, SqliteConnection connection) => builder
        .AddSqliteProvider(b => b.UseSqlite(connection));

    /// <summary>
    ///     Configures the messaging client to connect a SQLite database from a client.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, Action<SqliteOptions> configureOptions) => builder
        .AddSqliteProvider(b => b.UseSqlite(configureOptions));

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, IConfigurationSection configuration) => builder
        .AddSqliteProvider(b => b.UseSqlite(configuration));

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
            .AddSqliteProvider(builder.Name)
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddSqlite(messageType));
        return builder;
    }

    /// <summary>
    ///     Registers SQLite single provider and its dependencies.
    /// </summary>
    public static MessagingClientBuilder AddSqliteSingleProvider(this MessagingClientBuilder builder, Action<StorageBuilder> configureBuilder)
    {
        builder.Services
            .TryAddScoped<SqliteMessageHandlerProxy>()
            .AddStorage()
            .ConfigureStorage(builder.Name, b => b
                .AddPartitioned<int, IAbstractMessage>()
                .Add<int, CachingResult>())
            .ConfigureStorage(builder.Name, configureBuilder);
        return builder;
    }

    /// <summary>
    ///     Registers SQLite provider and its dependencies.
    /// </summary>
    public static MessagingClientBuilder AddSqliteProvider(this MessagingClientBuilder builder, Action<StorageBuilder> configureBuilder)
    {
        builder.Services
            .AddSqliteProvider(builder.Name)
            .ConfigureStorage(builder.Name, configureBuilder);
        return builder;
    }

    /// <summary>
    ///     Registers SQLite provider and its dependencies.
    /// </summary>
    public static IServiceCollection AddSqliteProvider(this IServiceCollection services, string name) => services
        .TryAddScoped<SqliteMessageHandlerProxy>()
        .AddStorage()
        .ConfigureStorage(name, b => b
            .AddSqlitePartitioned<int, IAbstractMessage>()
            .AddSqlite<int, CachingResult>());
}
