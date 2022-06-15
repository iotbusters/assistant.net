using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures messaging client to use MongoDB single provider.
    /// </summary>
    public static MessagingClientBuilder UseMongoSingleProvider(this MessagingClientBuilder builder)
    {
        builder.Services.AddMongoSingleProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use MongoDB provider dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseMongo(MessagingClientBuilder,string)"/> to configure;
    ///     It should be added if <see cref="AddMongo"/> wasn't configured on the start but configure <see cref="MessagingClientOptions"/> instead.
    /// </remarks>
    public static MessagingClientBuilder UseMongoProvider(this MessagingClientBuilder builder)
    {
        builder.Services.AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use MongoDB regular provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="connectionString">The MongoDB connection string.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) => builder
        .UseMongo(o => o.Connection(connectionString));

    /// <summary>
    ///     Configures messaging client to use MongoDB regular provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseMongo(configureOptions));
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use MongoDB regular provider.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="configuration">The application configuration values.</param>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        builder.Services.ConfigureStorage(builder.Name, b => b.UseMongo(configuration));
        return builder;
    }

    /// <summary>
    ///     Configures messaging client to use remote MongoDB based handler of <typeparamref name="TMessage"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseMongo(MessagingClientBuilder,string)"/> overloaded methods.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

    /// <summary>
    ///     Configures messaging client to use remote MongoDB based handler of <paramref name="messageType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of <see cref="UseMongo(MessagingClientBuilder,string)"/> overloaded methods.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    /// <param name="builder"/>
    /// <param name="messageType">The message type to find associated handler.</param>
    public static MessagingClientBuilder AddMongo(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddGeneric(messageType))
            .AddMongoProvider(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Configures MongoDB regular provider for storage based messaging handling dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of <see cref="UseMongo(MessagingClientBuilder,string)"/> overloaded methods to configure;
    ///     It should be added if <see cref="AddMongo"/> wasn't configured on the start.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddMongoProvider(this IServiceCollection services, string name) => services
        .AddStorage(name, b => b
            .AddMongo<IAbstractMessage, CachingResult>()
            .AddMongoPartitioned<int, IAbstractMessage>()
            .AddMongo<int, long>());

    /// <summary>
    ///     Configures MongoDB regular provider for storage based messaging handling dependencies.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of <see cref="UseMongo(MessagingClientBuilder,string)"/> overloaded methods to configure;
    ///     It should be added if <see cref="AddMongo"/> wasn't configured on the start.
    /// </remarks>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    private static void AddMongoSingleProvider(this IServiceCollection services, string name) => services
        .AddStorage(name, b => b
            .UseMongoSingleProvider()
            .AddSingle<IAbstractMessage, CachingResult>()
            .AddSinglePartitioned<int, IAbstractMessage>()
            .AddSingle<int, long>())
        .ConfigureMessagingClientOptions(name, o => o.UseGenericSingleProvider());
}
