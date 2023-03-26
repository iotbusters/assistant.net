using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
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
    ///     Configures messaging client to use SQLite single provider.
    /// </summary>
    public static MessagingClientBuilder UseSqliteSingleProvider(this MessagingClientBuilder builder)
    {
        builder.Services.AddSqliteSingleProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use SQLite provider dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseSqlite(MessagingClientBuilder,string)"/> to configure;
    ///     It should be added if <see cref="AddSqlite(MessagingClientBuilder,Type)"/> wasn't configured on the start but
    ///     configure <see cref="MessagingClientOptions"/> instead.
    /// </remarks>
    public static MessagingClientBuilder UseSqliteProvider(this MessagingClientBuilder builder)
    {
        builder.Services.AddSqliteProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use SQLite regular provider.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, string connectionString) => builder
        .UseSqlite(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures messaging client to use SQLite regular provider.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, Action<SqliteOptions> configureOptions)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseSqlite(configureOptions));
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use SQLite regular provider.
    /// </summary>
    public static MessagingClientBuilder UseSqlite(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseSqlite(configuration));
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use remote SQLite based handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseSqlite(MessagingClientBuilder,string)"/> method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddSqlite<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddSqlite(typeof(TMessage));

    /// <summary>
    ///     Configures messaging client to use remote SQLite based handler of <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseSqlite(MessagingClientBuilder,string)"/> method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    /// <param name="builder"/>
    /// <param name="messageType">The message type to find associated handler.</param>
    public static MessagingClientBuilder AddSqlite(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddGeneric(messageType))
            .AddSqliteProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use remote SQLite based handler of any message type except explicitly registered.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseSqlite(MessagingClientBuilder,string)"/> method.
    /// </remarks>
    public static MessagingClientBuilder AddSqlite(this MessagingClientBuilder builder)
    {
        builder.Services
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddGenericAny())
            .AddSqliteProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures SQLite regular provider for storage based messaging handling dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseSqlite(MessagingClientBuilder,string)"/> to configure;
    ///     It should be added if <see cref="AddSqlite(MessagingClientBuilder,Type)"/> wasn't configured on the start.
    /// </remarks>
    private static void AddSqliteProvider(this IServiceCollection services, string name) => services
        .AddStorage(name, b => b
            .AddSqlite<IAbstractMessage, CachingResult>() // CachingInterceptor's requirement
            .AddSqlite<string, CachingResult>() // GenericMessagingHandlerProxy's requirement
            .AddSqlitePartitioned<string, IAbstractMessage>() // GenericMessagingHandlerProxy's requirement
            .AddSqlite<string, RemoteHandlerModel>()); // GenericMessagingHandlerProxy's requirement

    /// <summary>
    ///     Configures SQLite single provider for storage based messaging handling dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseSqlite(MessagingClientBuilder,string)"/> to configure;
    ///     It should be added if <see cref="AddSqlite(MessagingClientBuilder,Type)"/> wasn't configured on the start.
    /// </remarks>
    private static void AddSqliteSingleProvider(this IServiceCollection services, string name) => services
        .AddStorage(name, b => b
            .UseSqliteSingleProvider()
            .AddSingle<IAbstractMessage, CachingResult>() // CachingInterceptor's requirement
            .AddSingle<string, CachingResult>() // GenericMessagingHandlerProxy's requirement
            .AddSinglePartitioned<string, IAbstractMessage>() // GenericMessagingHandlerProxy's requirement
            .AddSingle<string, RemoteHandlerModel>()) // GenericMessagingHandlerProxy's requirement
        .ConfigureMessagingClientOptions(name, o => o.UseGenericSingleProvider());
}
